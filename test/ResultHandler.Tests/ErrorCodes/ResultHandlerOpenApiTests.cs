#if NET10_0_OR_GREATER
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ResultHandler.AspNetCore.ErrorCodes;
using ResultHandler.AspNetCore.Extensions;
using ResultHandler.AspNetCore.OpenApi;
using ResultHandler.Facade;
using Xunit;

namespace ResultHandler.Tests.ErrorCodes;

public class ResultHandlerOpenApiTests
{
    [Fact]
    public async Task Envelopes_AreDocumentedAsWritten_RequiredMembersAndNothingNull()
    {
        using var document = await OpenApiDocumentAsync();
        var schemas = document.RootElement.GetProperty("components").GetProperty("schemas");

        var envelope = schemas.GetProperty("ResultEnvelopeOfWidget");
        Assert.Equal(["isSuccessful", "statusCode", "statusMessage", "resultData"], envelope.GetProperty("required").EnumerateArray().Select(name => name.GetString()));
        Assert.Equal("#/components/schemas/Widget", envelope.GetProperty("properties").GetProperty("resultData").GetProperty("$ref").GetString());
        Assert.Equal("string", envelope.GetProperty("properties").GetProperty("code").GetProperty("type").GetString());
        Assert.False(string.IsNullOrEmpty(envelope.GetProperty("properties").GetProperty("statusMessage").GetProperty("description").GetString()));
    }

    [Fact]
    public async Task Problems_RequireTheCode_WhenEveryProblemHasOne()
    {
        using var document = await OpenApiDocumentAsync();
        var problem = document.RootElement.GetProperty("components").GetProperty("schemas").GetProperty("ResultProblemDetails");

        Assert.Contains("code", problem.GetProperty("required").EnumerateArray().Select(name => name.GetString()));
        Assert.Equal("string", problem.GetProperty("properties").GetProperty("detail").GetProperty("type").GetString());
    }

    [Fact]
    public async Task FailureExamples_UseTheOperationsOwnCode_ElseAGeneralOne_AndLinkTheirType()
    {
        using var document = await OpenApiDocumentAsync();
        var responses = document.RootElement.GetProperty("paths").GetProperty("/widgets/{id}").GetProperty("get").GetProperty("responses");

        var notFound = Example(responses, "404");
        Assert.Equal(TestErrorCodes.WidgetNotFound, notFound.GetProperty("code").GetString());
        Assert.EndsWith("/errors/Widget.NotFound", notFound.GetProperty("type").GetString(), StringComparison.Ordinal);
        Assert.Equal("/widgets/{id}", notFound.GetProperty("instance").GetString());

        var serverError = Example(responses, "500");
        Assert.Equal("Server.Error", serverError.GetProperty("code").GetString());

        var invalid = Example(responses, "422");
        Assert.Equal("Validation Failed", invalid.GetProperty("title").GetString());
        Assert.Equal(JsonValueKind.Object, invalid.GetProperty("errors").ValueKind);
    }

    private static JsonElement Example(JsonElement responses, string status) =>
        responses.GetProperty(status).GetProperty("content").GetProperty("application/problem+json").GetProperty("example");

    private static async Task<JsonDocument> OpenApiDocumentAsync()
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { EnvironmentName = Environments.Development });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddResultProblemDetails();
        builder.Services.AddErrorCodeCatalog(options => options.AddAssembly(typeof(TestErrorCodes).Assembly));
        builder.Services.AddOpenApi(options => options.AddResultHandlerDocumentation(settings => settings.EveryProblemHasCode = true));

        await using var app = builder.Build();
        app.MapOpenApi();
        app.MapGet("/widgets/{id}", (int id, HttpContext httpContext) => Result.Success(new Widget(id, "Gear")).ToEnvelopedResult(httpContext))
            .WithTags("Widgets")
            .ProducesEnveloped<Widget>()
            .ProducesResultProblem(StatusCodes.Status404NotFound, StatusCodes.Status422UnprocessableEntity, StatusCodes.Status500InternalServerError);
        await app.StartAsync(TestContext.Current.CancellationToken);

        using var client = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };
        var json = await client.GetStringAsync("/openapi/v1.json", TestContext.Current.CancellationToken);
        await app.StopAsync(TestContext.Current.CancellationToken);
        return JsonDocument.Parse(json);
    }

    public sealed record Widget(int Id, string Name);
}
#endif
