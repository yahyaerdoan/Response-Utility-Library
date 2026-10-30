using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ResultHandler.AspNetCore.Exceptions;
using ResultHandler.AspNetCore.Extensions;
using ResultHandler.Core.Base;
using ResultHandler.Facade;
using Xunit;

namespace ResultHandler.Tests.Results;

public class ResultExceptionHandlerTests
{
    [Fact]
    public async Task UnmappedException_InProduction_Is500WithGenericDetail()
    {
        await using var app = await StartAsync(Environments.Production, _ => { }, () => throw new InvalidOperationException("Connection string 'Db' leaked."));

        var (status, body) = await GetAsync(app, "/throw");

        Assert.Equal(HttpStatusCode.InternalServerError, status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.6.1", body.GetProperty("type").GetString());
        Assert.Equal("An unexpected error occurred. Please try again later.", body.GetProperty("detail").GetString());
        Assert.Equal("/throw", body.GetProperty("instance").GetString());
        Assert.True(body.TryGetProperty("traceId", out _));
        Assert.False(body.TryGetProperty("exception", out _));
    }

    [Fact]
    public async Task UnmappedException_InDevelopment_ExposesTheMessage()
    {
        await using var app = await StartAsync(Environments.Development, _ => { }, () => throw new InvalidOperationException("Boom."));

        var (status, body) = await GetAsync(app, "/throw");

        Assert.Equal(HttpStatusCode.InternalServerError, status);
        Assert.Equal("Boom.", body.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task MappedException_UsesTheMappedResult()
    {
        await using var app = await StartAsync(
            Environments.Production,
            options => options.Map<InvalidOperationException>(exception => Result.Conflict(exception.Message).WithCode("Order.Locked")),
            () => throw new InvalidOperationException("The order is locked."));

        var (status, body) = await GetAsync(app, "/throw");

        Assert.Equal(HttpStatusCode.Conflict, status);
        Assert.Equal("The order is locked.", body.GetProperty("detail").GetString());
        Assert.Equal("Order.Locked", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task ClosestMappedBaseType_Wins()
    {
        await using var app = await StartAsync(
            Environments.Production,
            options => options
                .Map<Exception>(_ => Result.ServiceUnavailable())
                .Map<ArgumentException>(exception => Result.BadRequest(exception.Message)),
            () => throw new ArgumentOutOfRangeException("Id must be positive.", innerException: null));

        var (status, _) = await GetAsync(app, "/throw");

        Assert.Equal(HttpStatusCode.BadRequest, status);
    }

    [Fact]
    public async Task BadHttpRequestException_KeepsItsStatusCode_AndHidesTheMessageInProduction()
    {
        await using var app = await StartAsync(Environments.Production, _ => { }, () => throw new BadHttpRequestException("Body over 10 MB at offset 0x1F.", StatusCodes.Status413PayloadTooLarge));

        var (status, body) = await GetAsync(app, "/throw");

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, status);
        Assert.Equal("Payload Too Large", body.GetProperty("title").GetString());
        Assert.Equal("The request could not be read. Please check the request and try again.", body.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task MvcEndpoint_MappedValidationFailure_KeepsTheErrorsMap()
    {
        await using var app = await StartAsync(
            Environments.Production,
            options => options.Map<FormatException>(_ => OperationResult.Failure(new Dictionary<string, IReadOnlyList<string>> { ["Total"] = ["Total is not a number."] })),
            () => "unused",
            mvc: true);

        var (status, body) = await GetAsync(app, "/mvc/throw");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.Equal("Total is not a number.", body.GetProperty("errors").GetProperty("Total")[0].GetString());
    }

    [Fact]
    public async Task RegisteringTwice_AddsOneHandlerAndOneCustomization()
    {
        var customizations = 0;
        await using var app = await StartAsync(
            Environments.Production,
            _ => { },
            () => throw new InvalidOperationException("x"),
            configureServices: services =>
            {
                services.AddResultProblemDetails(options => options.CustomizeProblemDetails = _ => customizations++);
                services.AddResultExceptionHandler();
                services.AddResultProblemDetails();
            });

        _ = await GetAsync(app, "/throw");

        Assert.Single(app.Services.GetServices<IExceptionHandler>(), handler => handler is ResultExceptionHandler);
        Assert.Equal(1, customizations);
    }

    [Fact]
    public void MappingThatReturnsSuccess_IsRejected()
    {
        var options = new ResultExceptionHandlerOptions().Map<InvalidOperationException>(_ => Result.Success());

        var error = Assert.Throws<InvalidOperationException>(() => options.Resolve(new InvalidOperationException(), new DefaultHttpContext()));

        Assert.Contains("must return a failure", error.Message, StringComparison.Ordinal);
    }

    private static async Task<WebApplication> StartAsync(
        string environment,
        Action<ResultExceptionHandlerOptions> configure,
        Func<string> endpoint,
        bool mvc = false,
        Action<IServiceCollection>? configureServices = null)
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { EnvironmentName = environment });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddResultExceptionHandler(configure);
        configureServices?.Invoke(builder.Services);
        if (mvc)
        {
            builder.Services.AddControllers().AddApplicationPart(typeof(ThrowingController).Assembly);
        }

        var app = builder.Build();
        app.UseExceptionHandler();
        app.MapGet("/throw", endpoint);
        if (mvc)
        {
            app.MapControllers();
        }

        await app.StartAsync();
        return app;
    }

    private static async Task<(HttpStatusCode Status, JsonElement Body)> GetAsync(WebApplication app, string path)
    {
        using var client = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };
        using var response = await client.GetAsync(path);
        var json = await response.Content.ReadAsStringAsync();
        Assert.StartsWith("application/problem+json", response.Content.Headers.ContentType?.ToString(), StringComparison.Ordinal);
        using var document = JsonDocument.Parse(json);
        return (response.StatusCode, document.RootElement.Clone());
    }
}
