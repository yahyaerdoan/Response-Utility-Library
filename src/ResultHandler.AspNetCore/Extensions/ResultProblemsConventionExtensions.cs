using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using ResultHandler.AspNetCore.OpenApi;

namespace ResultHandler.AspNetCore.Extensions;

/// <summary>Documents, for OpenAPI generators, the RFC 9457 error responses a result-returning endpoint can produce, without adding metadata to every endpoint by hand.</summary>
public static class ResultProblemsConventionExtensions
{
    private const string _problemJsonContentType = "application/problem+json";

    /// <summary>Adds <c>application/problem+json</c> response metadata to every endpoint in <paramref name="builder"/>: 500 always, 401 and 403 when the endpoint requires authorization, and 400 and 422 when it reads a request body. 400/422 are described as <see cref="ResultValidationProblemDetails"/>, the rest as <see cref="ResultProblemDetails"/>. Status codes the endpoint already declares are left alone. Works with Microsoft.AspNetCore.OpenApi and Swashbuckle alike.</summary>
    /// <example>
    /// <code>
    /// var api = app.MapGroup("/api").ProducesResultProblems();
    /// api.MapGet("/products/{id:int}", (int id, ProductService service) =&gt; service.Find(id).ToResult());
    /// </code>
    /// </example>
    public static TBuilder ProducesResultProblems<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Finally(endpoint =>
        {
            var metadata = endpoint.Metadata;
            var requiresAuthorization = metadata.OfType<IAuthorizeData>().Any() && !metadata.OfType<IAllowAnonymous>().Any();
            var readsBody = metadata.OfType<IAcceptsMetadata>().Any();

            if (requiresAuthorization)
            {
                AddProblem(metadata, StatusCodes.Status401Unauthorized);
                AddProblem(metadata, StatusCodes.Status403Forbidden);
            }

            if (readsBody)
            {
                AddProblem(metadata, StatusCodes.Status400BadRequest);
                AddProblem(metadata, StatusCodes.Status422UnprocessableEntity);
            }

            AddProblem(metadata, StatusCodes.Status500InternalServerError);
        });

        return builder;
    }

    /// <summary>Documents <c>application/problem+json</c> responses for endpoint-specific failures such as 404 or 409, with the same schemas as <see cref="ProducesResultProblems{TBuilder}"/>. Status codes the endpoint already declares are left alone.</summary>
    /// <example>
    /// <code>
    /// api.MapGet("/products/{id:int}", (int id, ProductService service) =&gt; service.Find(id).ToEnvelopedResult())
    ///     .ProducesEnveloped&lt;ProductResponse&gt;()
    ///     .ProducesResultProblem(StatusCodes.Status404NotFound);
    /// </code>
    /// </example>
    public static TBuilder ProducesResultProblem<TBuilder>(this TBuilder builder, params int[] statusCodes)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(statusCodes);

        builder.Add(endpoint =>
        {
            foreach (var statusCode in statusCodes)
            {
                AddProblem(endpoint.Metadata, statusCode);
            }
        });

        return builder;
    }

    private static void AddProblem(IList<object> metadata, int statusCode)
    {
        if (metadata.OfType<IProducesResponseTypeMetadata>().Any(existing => existing.StatusCode == statusCode))
        {
            return;
        }

        var schema = statusCode is StatusCodes.Status400BadRequest or StatusCodes.Status422UnprocessableEntity
            ? typeof(ResultValidationProblemDetails)
            : typeof(ResultProblemDetails);
        metadata.Add(new ProducesResponseTypeMetadata(statusCode, schema, [_problemJsonContentType]));
    }
}
