using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;
using ResultHandler.FluentValidation;
using Xunit;
using ReceiptRequest = ResultHandler.Tests.Results.ValidatedController.ReceiptRequest;

namespace ResultHandler.Tests.Results;

public class FluentValidationTests
{
    [Fact]
    public void ToFieldErrors_GroupsByProperty_WithDistinctMessages()
    {
        var validation = new ValidationResult(
        [
            new ValidationFailure("Number", "Number is required."),
            new ValidationFailure("Number", "Number is required."),
            new ValidationFailure("Lines[0].Total", "Total must be positive."),
            new ValidationFailure(string.Empty, "The receipt is closed."),
        ]);

        var fieldErrors = validation.ToFieldErrors();

        Assert.Equal(["Number is required."], fieldErrors["Number"]);
        Assert.Equal(["Total must be positive."], fieldErrors["Lines[0].Total"]);
        Assert.Equal(["The receipt is closed."], fieldErrors[string.Empty]);
    }

    [Fact]
    public void ToOperationResult_IsSuccessWhenValid_And422WithFieldErrorsWhenNot()
    {
        Assert.True(new ValidationResult().ToOperationResult().IsSuccessful);

        var failure = new ValidationResult([new ValidationFailure("Number", "Number is required.")]).ToOperationResult();

        Assert.False(failure.IsSuccessful);
        Assert.Equal(ResultStatus.UnprocessableContent, failure.Status);
        Assert.Equal(["Number is required."], failure.Errors["Number"]);
    }

    [Fact]
    public void ToFailure_ReturnsTheRequestedResultType()
    {
        var failure = new ValidationResult([new ValidationFailure("Number", "Number is required.")]).ToFailure<OperationDataResult<int>>();

        Assert.False(failure.IsSuccessful);
        Assert.Equal(["Number is required."], failure.Errors["Number"]);
    }

    [Fact]
    public void ToFailure_OnAValidResult_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new ValidationResult().ToFailure<OperationResult>());
    }

    [Fact]
    public async Task MinimalApi_InvalidRequest_Is422WithTheErrorsMap_AndTheHandlerDoesNotRun()
    {
        var handlerRuns = 0;
        await using var app = await StartAsync(app => app.MapGroup("/api").WithResultValidation()
            .MapPost("/receipts", (ReceiptRequest request) =>
            {
                handlerRuns++;
                return request.Number;
            }));

        var (status, body) = await PostAsync(app, "/api/receipts", new ReceiptRequest(string.Empty, -1));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.Equal("Number is required.", body.GetProperty("errors").GetProperty("Number")[0].GetString());
        Assert.Equal("Total must be positive.", body.GetProperty("errors").GetProperty("Total")[0].GetString());
        Assert.Equal(0, handlerRuns);
    }

    [Fact]
    public async Task MinimalApi_ValidRequest_ReachesTheHandler()
    {
        await using var app = await StartAsync(app => app.MapPost("/receipts", (ReceiptRequest request) => request.Number).WithResultValidation());

        using var client = Client(app);
        using var response = await client.PostAsJsonAsync("/receipts", new ReceiptRequest("R-1", 10), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("R-1", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task MinimalApi_RunsAsyncRules_AndMergesEveryValidatorForTheType()
    {
        await using var app = await StartAsync(
            app => app.MapPost("/receipts", (ReceiptRequest request) => request.Number).WithResultValidation(),
            services => services.AddScoped<IValidator<ReceiptRequest>, ReservedNumberValidator>());

        var (status, body) = await PostAsync(app, "/receipts", new ReceiptRequest("TAKEN", -1));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.Equal("Number is already used.", body.GetProperty("errors").GetProperty("Number")[0].GetString());
        Assert.Equal("Total must be positive.", body.GetProperty("errors").GetProperty("Total")[0].GetString());
    }

    [Fact]
    public async Task MinimalApi_EndpointWithoutValidatedParameters_IsUnaffected()
    {
        await using var app = await StartAsync(app => app.MapGroup("/api").WithResultValidation().MapGet("/ping", () => "pong"));

        using var client = Client(app);

        Assert.Equal("pong", await client.GetStringAsync("/api/ping", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Mvc_InvalidRequest_Is422WithTheErrorsMap()
    {
        await using var app = await StartAsync(app => app.MapControllers(), mvc: true);

        var (status, body) = await PostAsync(app, "/mvc/receipts", new ReceiptRequest(string.Empty, 5));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.Equal("Number is required.", body.GetProperty("errors").GetProperty("Number")[0].GetString());
    }

    [Fact]
    public async Task Mvc_ValidRequest_RunsTheAction()
    {
        await using var app = await StartAsync(app => app.MapControllers(), mvc: true);

        using var client = Client(app);
        using var response = await client.PostAsJsonAsync("/mvc/receipts", new ReceiptRequest("R-2", 5), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<WebApplication> StartAsync(Action<WebApplication> map, Action<IServiceCollection>? configureServices = null, bool mvc = false)
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { EnvironmentName = Environments.Development });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddScoped<IValidator<ReceiptRequest>, ReceiptRequestValidator>();
        configureServices?.Invoke(builder.Services);
        if (mvc)
        {
            builder.Services.AddControllers().AddApplicationPart(typeof(ValidatedController).Assembly).AddResultValidation();
        }

        var app = builder.Build();
        map(app);
        await app.StartAsync();
        return app;
    }

    private static HttpClient Client(WebApplication app) => new() { BaseAddress = new Uri(app.Urls.First()) };

    private static async Task<(HttpStatusCode Status, JsonElement Body)> PostAsync(WebApplication app, string path, ReceiptRequest request)
    {
        using var client = Client(app);
        using var response = await client.PostAsJsonAsync(path, request, TestContext.Current.CancellationToken);
        Assert.StartsWith("application/problem+json", response.Content.Headers.ContentType?.ToString(), StringComparison.Ordinal);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        return (response.StatusCode, document.RootElement.Clone());
    }

    public sealed class ReceiptRequestValidator : AbstractValidator<ReceiptRequest>
    {
        public ReceiptRequestValidator()
        {
            RuleFor(request => request.Number).NotEmpty().WithMessage("Number is required.");
            RuleFor(request => request.Total).GreaterThan(0).WithMessage("Total must be positive.");
        }
    }

    public sealed class ReservedNumberValidator : AbstractValidator<ReceiptRequest>
    {
        public ReservedNumberValidator()
        {
            RuleFor(request => request.Number)
                .MustAsync(async (number, cancellationToken) =>
                {
                    await Task.Delay(1, cancellationToken);
                    return number != "TAKEN";
                })
                .WithMessage("Number is already used.");
        }
    }
}
