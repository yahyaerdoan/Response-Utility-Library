using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using ResultHandler.AspNetCore.ErrorCodes;
using ResultHandler.AspNetCore.Extensions;
using ResultHandler.Core.Base;
using ResultHandler.Facade;
using ResultHandler.Mapping;

namespace ResultHandler.AspNetCore.OpenApi;

/// <summary>Gives each documented failure an example written the way a failed result is. With the error code catalog registered, the example
/// carries a code: one of the operation's own subject when its tags name one (Order.NotFound under "Orders"), else a general code for the status.</summary>
internal sealed class ProblemExampleTransformer : IOpenApiOperationTransformer
{
    private const string ProblemJson = "application/problem+json";
    private const string ExampleTraceId = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";

    private static readonly Dictionary<string, IReadOnlyList<string>> ExampleFieldErrors = new(StringComparer.Ordinal)
    {
        ["name"] = ["'Name' must not be empty."],
    };

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var services = context.ApplicationServices;
        var catalog = services.GetService<ErrorCodeCatalog>();
        var catalogOptions = services.GetService<IOptions<ErrorCodeCatalogOptions>>()?.Value;
        var request = services.GetService<IHttpContextAccessor>()?.HttpContext?.Request;
        var subjects = Subjects(operation.Tags?.Select(tag => tag.Name ?? string.Empty) ?? []);
        var instance = "/" + context.Description.RelativePath;

        foreach (var (key, response) in operation.Responses ?? [])
        {
            if (int.TryParse(key, out var status) && status >= 400 && response.Content is { } content && content.TryGetValue(ProblemJson, out var media))
            {
                var code = catalog is null ? null : CodeFor(catalog, status, subjects);
                var type = code is not null && request is not null && catalogOptions is { LinkProblemTypes: true } ? catalogOptions.DocumentationUrl(request, code) : null;
                media.Example = Example(status, instance, code, type);
            }
        }

        return Task.CompletedTask;
    }

    private static JsonObject Example(int status, string instance, string? code, string? type)
    {
        var failure = status == StatusCodes.Status422UnprocessableEntity
            ? OperationResult.Failure(ExampleFieldErrors)
            : OperationResult.Failure(ReasonPhrases.GetReasonPhrase(status), string.Empty, status.ToResultStatus());
        var problem = (code is null ? failure : failure.WithCode(code)).ToProblemDetails();

        var example = new JsonObject
        {
            ["type"] = type ?? problem.Type,
            ["title"] = problem.Title,
            ["status"] = problem.Status,
            ["instance"] = instance,
            ["traceId"] = ExampleTraceId,
        };

        if (problem.Extensions.TryGetValue(ResultProblemFields.Code, out var written) && written is string writtenCode)
        {
            example[ResultProblemFields.Code] = writtenCode;
        }

        if (problem is HttpValidationProblemDetails { Errors.Count: > 0 } validation)
        {
            example["errors"] = new JsonObject(validation.Errors.Select(field =>
                KeyValuePair.Create<string, JsonNode?>(field.Key, new JsonArray([.. field.Value.Select(message => JsonValue.Create(message))]))));
        }

        return example;
    }

    private static string? CodeFor(ErrorCodeCatalog catalog, int status, List<string> subjects)
    {
        var candidates = catalog.All.Where(entry => entry.Status == status)
            .OrderBy(entry => entry.Code.EndsWith(".NotFound", StringComparison.Ordinal) ? 0 : 1)
            .ToList();

        return subjects.Select(subject => candidates.Find(entry => entry.Code.StartsWith(subject + ".", StringComparison.Ordinal))?.Code).FirstOrDefault(code => code is not null)
            ?? candidates.Find(entry => entry.General)?.Code
            ?? candidates.FirstOrDefault()?.Code;
    }

    private static List<string> Subjects(IEnumerable<string> tags) =>
        [.. tags.SelectMany(tag => tag.Split('/').Reverse()).SelectMany(segment => new[] { Singular(segment), segment }).Distinct(StringComparer.Ordinal)];

    private static string Singular(string word)
    {
        if (word.EndsWith("ies", StringComparison.Ordinal))
        {
            return word[..^3] + "y";
        }

        if (word.EndsWith("sses", StringComparison.Ordinal) || word.EndsWith("xes", StringComparison.Ordinal))
        {
            return word[..^2];
        }

        return word.EndsWith('s') ? word[..^1] : word;
    }
}
