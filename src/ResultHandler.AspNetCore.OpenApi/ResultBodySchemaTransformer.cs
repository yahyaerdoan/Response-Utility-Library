using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using ResultHandler.AspNetCore.Envelopes;
using ResultHandler.Core.Enums;

namespace ResultHandler.AspNetCore.OpenApi;

/// <summary>Describes the bodies ResultHandler writes as they are written: the generator sees every member of them as nullable and can't read
/// this package's comments, but none is ever written as null (an empty one is left out), and the always-written ones are required.</summary>
internal sealed class ResultBodySchemaTransformer(ResultHandlerOpenApiOptions options) : IOpenApiSchemaTransformer, IOpenApiDocumentTransformer
{
    private const string ResultData = "resultData";

    private static readonly Dictionary<string, string> ProblemDescriptions = new(StringComparer.Ordinal)
    {
        ["type"] = "URI identifying the problem type: the documentation of the error code, or the RFC section of the status.",
        ["title"] = "The HTTP status's reason phrase.",
        ["status"] = "The HTTP status code.",
        ["detail"] = "What went wrong, for a person to read; left out when the status says it all.",
        ["instance"] = "Path of the request that failed.",
        ["traceId"] = "Identifier of the request; include it when reporting a problem.",
        ["code"] = "Stable code to branch on; \"type\" links to its documentation.",
        ["errors"] = "Each invalid field's messages; messages not tied to a field are under the \"\" key.",
    };

    private static readonly Dictionary<string, string> EnvelopeDescriptions = new(StringComparer.Ordinal)
    {
        ["isSuccessful"] = "Always true: a failed request answers with Problem Details instead.",
        ["statusCode"] = "The HTTP status code.",
        ["statusMessage"] = "Short summary of the outcome, for a person to read.",
        ["detail"] = "More about the outcome; left out when there is nothing to add.",
        ["code"] = "Code for the outcome; left out when there is none.",
        [ResultData] = "The data the request returns.",
    };

    private static readonly string[] EnvelopeMembers = ["isSuccessful", "statusCode", "statusMessage"];

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;
        if (type == typeof(ResultStatus))
        {
            schema.Type = JsonSchemaType.Integer;
            schema.Format = "int32";
        }
        else if (type == typeof(ResultProblemDetails) || type == typeof(ResultValidationProblemDetails))
        {
            Describe(schema, ProblemMembers(), ProblemDescriptions);
        }
        else if (type == typeof(ResultEnvelope))
        {
            Describe(schema, EnvelopeMembers, EnvelopeDescriptions);
        }
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ResultEnvelope<>))
        {
            Describe(schema, [.. EnvelopeMembers, ResultData], EnvelopeDescriptions);
        }

        return Task.CompletedTask;
    }

    /// <summary>Runs once the document is complete: the generator wraps a member that points at another schema in "null or ..." only after the
    /// schema transformers ran, so a data envelope's resultData is unwrapped here.</summary>
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        foreach (var schema in document.Components?.Schemas?.Values.OfType<OpenApiSchema>() ?? [])
        {
            if (schema.Required?.Contains(ResultData) == true && schema.Properties?.TryGetValue(ResultData, out var member) == true)
            {
                schema.Properties[ResultData] = Described(NonNull(member), EnvelopeDescriptions[ResultData]);
            }
        }

        return Task.CompletedTask;
    }

    private static void Describe(OpenApiSchema schema, string[] alwaysWritten, Dictionary<string, string> descriptions)
    {
        if (schema.Properties is not { } members)
        {
            return;
        }

        foreach (var name in members.Keys.ToList())
        {
            members[name] = Described(NonNull(members[name]), descriptions.GetValueOrDefault(name));
        }

        schema.Required = new HashSet<string>(alwaysWritten, StringComparer.Ordinal);
    }

    private static IOpenApiSchema NonNull(IOpenApiSchema member)
    {
        if (member is OpenApiSchema { OneOf: { Count: 2 } choices } && choices.Count(IsNull) == 1)
        {
            return choices.First(choice => !IsNull(choice));
        }

        if (member is OpenApiSchema { Type: { } type } inline)
        {
            inline.Type = type & ~JsonSchemaType.Null;
        }

        return member;
    }

    private static bool IsNull(IOpenApiSchema schema) => schema is OpenApiSchema { Type: JsonSchemaType.Null };

    private static IOpenApiSchema Described(IOpenApiSchema member, string? description)
    {
        switch (member)
        {
            case OpenApiSchema inline when description is not null:
                inline.Description = description;
                break;
            case OpenApiSchemaReference reference when description is not null:
                reference.Description = description;
                break;
        }

        return member;
    }

    private string[] ProblemMembers() =>
        options.EveryProblemHasCode ? ["type", "title", "status", "instance", "traceId", "code"] : ["type", "title", "status", "instance", "traceId"];
}
