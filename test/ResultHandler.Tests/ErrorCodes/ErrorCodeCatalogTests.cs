using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ResultHandler.AspNetCore.ErrorCodes;
using ResultHandler.AspNetCore.Exceptions;
using ResultHandler.AspNetCore.Extensions;
using ResultHandler.Facade;
using Xunit;

namespace ResultHandler.Tests.ErrorCodes;

public class ErrorCodeCatalogTests
{
    [Fact]
    public void Catalog_ReadsDocumentedConstants_AndAlwaysHasResultHandlersOwnCodes()
    {
        var catalog = new ErrorCodeCatalog(new ErrorCodeCatalogOptions().AddAssembly(typeof(TestErrorCodes).Assembly).Assemblies);

        var widget = catalog.Find(TestErrorCodes.WidgetNotFound);
        Assert.NotNull(widget);
        Assert.Equal(404, widget.Status);
        Assert.Equal("Not Found", widget.Title);
        Assert.Equal("The widget doesn't exist.", widget.Description);
        Assert.False(widget.General);
        Assert.Null(catalog.Find(TestErrorCodes.Undocumented));
        Assert.True(catalog.Find(ResultErrorCodes.ServerError)?.General);
        Assert.Equal(catalog.All.Select(entry => entry.Code).Order(StringComparer.Ordinal), catalog.All.Select(entry => entry.Code));
    }

    [Fact]
    public void AddAssemblyTree_FollowsReferencesWithTheGivenPrefix()
    {
        var options = new ErrorCodeCatalogOptions().AddAssemblyTree(typeof(TestErrorCodes).Assembly, "ResultHandler");

        Assert.Contains(options.Assemblies, assembly => assembly.GetName().Name == "ResultHandler");
        Assert.Contains(options.Assemblies, assembly => assembly.GetName().Name == "ResultHandler.AspNetCore");
        Assert.DoesNotContain(options.Assemblies, assembly => assembly.GetName().Name?.StartsWith("Microsoft.", StringComparison.Ordinal) == true);
    }

    [Fact]
    public async Task MapErrorCodes_ServesTheListAndOneCode_AndAnUnknownCodeIsAProblemWithItsOwnCode()
    {
        await using var app = await StartAsync();
        using var client = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };
        var cancellationToken = TestContext.Current.CancellationToken;

        using var list = JsonDocument.Parse(await client.GetStringAsync("/errors", cancellationToken));
        Assert.Contains(list.RootElement.EnumerateArray(), entry => entry.GetProperty("code").GetString() == TestErrorCodes.WidgetLocked);

        using var one = JsonDocument.Parse(await client.GetStringAsync("/errors/Widget.Locked", cancellationToken));
        Assert.Equal(409, one.RootElement.GetProperty("status").GetInt32());

        using var unknown = await client.GetAsync("/errors/Nope.Nope", cancellationToken);
        using var problem = JsonDocument.Parse(await unknown.Content.ReadAsStringAsync(cancellationToken));
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Equal(ResultErrorCodes.UnknownErrorCode, problem.RootElement.GetProperty(ResultProblemFields.Code).GetString());
        Assert.Equal($"{app.Urls.First()}/errors/{ResultErrorCodes.UnknownErrorCode}", problem.RootElement.GetProperty("type").GetString());
    }

    [Fact]
    public async Task LinkProblemTypes_PointsACatalogedCodeAtItsDocumentation_AndLeavesOtherCodesAlone()
    {
        await using var app = await StartAsync(map: application =>
        {
            application.MapGet("/widgets/missing", (HttpContext httpContext) => Result.NotFound("No widget.").WithCode(TestErrorCodes.WidgetNotFound).ToResult(httpContext));
            application.MapGet("/widgets/odd", (HttpContext httpContext) => Result.NotFound("Odd.").WithCode(TestErrorCodes.Undocumented).ToResult(httpContext));
        });
        using var client = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };
        var cancellationToken = TestContext.Current.CancellationToken;

        using var linked = JsonDocument.Parse(await (await client.GetAsync("/widgets/missing", cancellationToken)).Content.ReadAsStringAsync(cancellationToken));
        using var unlinked = JsonDocument.Parse(await (await client.GetAsync("/widgets/odd", cancellationToken)).Content.ReadAsStringAsync(cancellationToken));

        Assert.Equal($"{app.Urls.First()}/errors/Widget.NotFound", linked.RootElement.GetProperty("type").GetString());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", unlinked.RootElement.GetProperty("type").GetString());
    }

    [Fact]
    public async Task LinkProblemTypes_Off_KeepsTheStatusType()
    {
        await using var app = await StartAsync(
            options => options.LinkProblemTypes = false,
            application => application.MapGet("/widgets/missing", (HttpContext httpContext) => Result.NotFound("No widget.").WithCode(TestErrorCodes.WidgetNotFound).ToResult(httpContext)));
        using var client = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };
        var cancellationToken = TestContext.Current.CancellationToken;

        using var problem = JsonDocument.Parse(await (await client.GetAsync("/widgets/missing", cancellationToken)).Content.ReadAsStringAsync(cancellationToken));

        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problem.RootElement.GetProperty("type").GetString());
    }

    private static async Task<WebApplication> StartAsync(Action<ErrorCodeCatalogOptions>? configure = null, Action<WebApplication>? map = null)
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { EnvironmentName = Environments.Development });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddResultProblemDetails();
        builder.Services.AddErrorCodeCatalog(options =>
        {
            options.AddAssembly(typeof(TestErrorCodes).Assembly);
            configure?.Invoke(options);
        });

        var app = builder.Build();
        app.MapErrorCodes();
        map?.Invoke(app);
        await app.StartAsync();
        return app;
    }
}
