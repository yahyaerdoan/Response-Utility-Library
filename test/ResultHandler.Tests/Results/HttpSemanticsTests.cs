using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ResultHandler.AspNetCore.Exceptions;
using ResultHandler.AspNetCore.Extensions;
using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;
using ResultHandler.Facade;
using ResultHandler.Implementations.Success;
using Xunit;

namespace ResultHandler.Tests.Results;

public class HttpSemanticsTests
{
    [Fact]
    public async Task ServerErrors_AreLoggedByTheMiddleware()
    {
        var logs = new List<string>();
        await using var app = await StartAsync(Environments.Production, app => app.MapGet("/boom", string () => throw new InvalidOperationException("boom")), logs: logs);

        _ = await SendAsync(app, HttpMethod.Get, "/boom");

        Assert.Contains(logs, log => log.StartsWith("Error Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware", StringComparison.Ordinal));
    }

#if NET10_0_OR_GREATER
    [Fact]
    public async Task ClientErrors_AreNotLoggedAsErrors()
    {
        var logs = new List<string>();
        await using var app = await StartAsync(Environments.Development, app => app.MapGet("/items/{id}", (Guid id) => id), logs: logs);

        _ = await SendAsync(app, HttpMethod.Get, "/items/abc");

        Assert.DoesNotContain(logs, log => log.StartsWith("Error Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware", StringComparison.Ordinal));
    }
#endif

    [Theory]
    [InlineData("/items/abc", null, "Id", "The value is missing or has an invalid format.")]
    [InlineData("/page?page=abc", null, "Page", "The value is missing or has an invalid format.")]
    [InlineData("/page", null, "Page", "This field is required.")]
    [InlineData("/orders", "{\"serviceId\":\"fger98\",\"quantity\":1}", "ServiceId", "The value is missing or has an invalid format.")]
    [InlineData("/orders", "{\"serviceId\":\"17120728-c683-4914-a532-05897f33d0ba\",\"lines\":[{\"quantity\":\"two\"}]}", "Lines[0].Quantity", "The value is missing or has an invalid format.")]
    [InlineData("/orders", "{\"serviceId\":", "", "The request body is not valid JSON.")]
    [InlineData("/orders", "", "", "A request body is required.")]
    [InlineData("/required", "{}", "Name", "This field is required.")]
    public async Task BindingFailures_NameTheField(string path, string? body, string field, string message)
    {
        await using var app = await StartAsync(Environments.Development, MapBindingEndpoints);

        var (status, json) = await SendAsync(app, body is null ? HttpMethod.Get : HttpMethod.Post, path, body);

        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.Equal(message, json.GetProperty("errors").GetProperty(field)[0].GetString());
    }

    [Fact]
    public async Task BindingFailures_NameTheFieldInProduction_WithAGenericDetail()
    {
        await using var app = await StartAsync(
            Environments.Production,
            MapBindingEndpoints,
            services => services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true));

        var (status, json) = await SendAsync(app, HttpMethod.Get, "/items/abc");

        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.Equal("The value is missing or has an invalid format.", json.GetProperty("errors").GetProperty("Id")[0].GetString());
        Assert.Equal("The request could not be read. Please check the request and try again.", json.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task UpstreamFailures_Are502And504()
    {
        await using var app = await StartAsync(Environments.Production, app =>
        {
            app.MapGet("/upstream", string () => throw new HttpRequestException("Stripe returned 500"));
            app.MapGet("/timeout", string () => throw new TimeoutException("Brevo did not answer"));
            app.MapGet("/client-timeout", string () => throw new TaskCanceledException("HttpClient.Timeout elapsed", new TimeoutException()));
            app.MapGet("/cancelled", string () => throw new TaskCanceledException("Cancelled by the app"));
        });

        Assert.Equal(HttpStatusCode.BadGateway, (await SendAsync(app, HttpMethod.Get, "/upstream")).Status);
        Assert.Equal(HttpStatusCode.GatewayTimeout, (await SendAsync(app, HttpMethod.Get, "/timeout")).Status);
        Assert.Equal(HttpStatusCode.GatewayTimeout, (await SendAsync(app, HttpMethod.Get, "/client-timeout")).Status);
        Assert.Equal(HttpStatusCode.InternalServerError, (await SendAsync(app, HttpMethod.Get, "/cancelled")).Status);
    }

    [Fact]
    public void Resolve_IsPublic_AndAppliesTheClosestMapping()
    {
        var options = new ResultExceptionHandlerOptions().Map<ArgumentException>(exception => Result.BadRequest(exception.Message));

        var result = options.Resolve(new ArgumentOutOfRangeException("Too many.", innerException: null), new DefaultHttpContext());

        Assert.Equal(ResultStatus.BadRequest, result.Status);
        Assert.Equal(ResultStatus.BadGateway, options.Resolve(new HttpRequestException(), new DefaultHttpContext()).Status);
    }

    [Fact]
    public void InterimStatuses_CannotBeTheFinalResponse()
    {
        Assert.Throws<InvalidOperationException>(() => new SuccessResult("Continue.", ResultStatus.Continue).ToResult());
        Assert.Throws<InvalidOperationException>(() => new SuccessResult("Processing.", ResultStatus.Processing).ToActionResult());
        Assert.Throws<InvalidOperationException>(() => new SuccessResult("Early hints.", ResultStatus.EarlyHints).ToEnvelopedResult());
    }

    [Fact]
    public async Task Created_WithLocation_WritesTheHeaderAndTheBody()
    {
        var httpContext = CreateHttpContext();

        await Result.Created(5).WithLocation("/api/orders/5").ToResult().ExecuteAsync(httpContext);

        Assert.Equal(201, httpContext.Response.StatusCode);
        Assert.Equal("/api/orders/5", httpContext.Response.Headers.Location.ToString());
        httpContext.Response.Body.Position = 0;
        using var reader = new StreamReader(httpContext.Response.Body);
        Assert.Equal("5", await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Redirects_WriteTheLocationHeader()
    {
        var httpContext = CreateHttpContext();

        await Result.MovedPermanently("https://example.com/new").ToResult().ExecuteAsync(httpContext);

        Assert.Equal(301, httpContext.Response.StatusCode);
        Assert.Equal("https://example.com/new", httpContext.Response.Headers.Location.ToString());
    }

    [Fact]
    public async Task Mvc_Created_WithLocation_WritesTheHeader()
    {
        var httpContext = CreateHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        await Result.Created().WithLocation("/api/orders/7").ToActionResult().ExecuteResultAsync(actionContext);

        Assert.Equal(201, httpContext.Response.StatusCode);
        Assert.Equal("/api/orders/7", httpContext.Response.Headers.Location.ToString());
    }

    [Fact]
    public void ResultsWithoutLocation_AreNotWrapped()
    {
        Assert.IsType<JsonHttpResult<int>>(Result.Created(5).ToResult());
    }

    [Fact]
    public async Task Mvc_UnreadableBody_Is400WithFieldKeys()
    {
        await using var app = await StartAsync(Environments.Production, app => app.MapControllers(), mvc: true);

        var (status, json) = await SendAsync(app, HttpMethod.Post, "/mvc/orders", "{\"serviceId\":\"fger98\",\"quantity\":1}");

        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.Equal("The value is missing or has an invalid format.", json.GetProperty("errors").GetProperty("ServiceId")[0].GetString());
    }

    [Fact]
    public async Task Mvc_ValidationAttributeFailure_Is422()
    {
        await using var app = await StartAsync(Environments.Production, app => app.MapControllers(), mvc: true);

        var (status, json) = await SendAsync(app, HttpMethod.Post, "/mvc/orders", "{\"serviceId\":\"17120728-c683-4914-a532-05897f33d0ba\",\"quantity\":50}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.True(json.GetProperty("errors").TryGetProperty("Quantity", out _));
    }

    private static void MapBindingEndpoints(WebApplication app)
    {
        app.MapGet("/items/{id}", (Guid id) => id);
        app.MapGet("/page", (int page) => page);
        app.MapPost("/orders", (OrderBody order) => order.ServiceId);
        app.MapPost("/required", (RequiredBody body) => body.Name);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().AddLogging().AddControllers().Services.BuildServiceProvider(),
        };
        httpContext.Response.Body = new MemoryStream();
        return httpContext;
    }

    private static async Task<WebApplication> StartAsync(
        string environment,
        Action<WebApplication> map,
        Action<IServiceCollection>? configureServices = null,
        bool mvc = false,
        List<string>? logs = null)
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { EnvironmentName = environment });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        if (logs is not null)
        {
            builder.Services.AddSingleton<ILoggerProvider>(_ => new CapturingLoggerProvider(logs));
        }

        builder.Services.AddResultExceptionHandler();
        configureServices?.Invoke(builder.Services);
        if (mvc)
        {
            builder.Services.AddControllers().AddApplicationPart(typeof(ModelStateController).Assembly).AddResultModelStateResponses();
        }

        var app = builder.Build();
        app.UseExceptionHandler();
        map(app);
        await app.StartAsync();
        return app;
    }

    private static async Task<(HttpStatusCode Status, JsonElement Json)> SendAsync(WebApplication app, HttpMethod method, string path, string? body = null)
    {
        using var client = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };
        using var request = new HttpRequestMessage(method, path);
        if (body is not null)
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        var text = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        if (text.Length == 0)
        {
            return (response.StatusCode, default);
        }

        using var document = JsonDocument.Parse(text);
        return (response.StatusCode, document.RootElement.Clone());
    }

    public sealed record OrderBody(Guid ServiceId, IReadOnlyList<OrderLine>? Lines);

    public sealed record OrderLine(int Quantity);

    public sealed class RequiredBody
    {
        public required string Name { get; init; }
    }

    private sealed class CapturingLoggerProvider(List<string> sink) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new CapturingLogger(sink, categoryName);

        public void Dispose()
        {
        }

        private sealed class CapturingLogger(List<string> sink, string category) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull
                => null;

            public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                lock (sink)
                {
                    sink.Add($"{logLevel} {category} {formatter(state, exception)}");
                }
            }
        }
    }
}
