using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace ResultHandler.AspNetCore.Extensions;

/// <summary>Documents, for OpenAPI generators, the RFC 9457 error responses a result-returning endpoint can produce, without adding metadata to every endpoint by hand.</summary>
public static class ResultProblemsConventionExtensions
{
    private const string _problemJsonContentType = "application/problem+json";

    /// <summary>Adds <c>application/problem+json</c> response metadata to every endpoint in <paramref name="builder"/>: 500 always, 401 and 403 when the endpoint requires authorization, and 422 when it reads a request body. Status codes the endpoint already declares are left alone. Works with Microsoft.AspNetCore.OpenApi and Swashbuckle alike.</summary>
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
                AddProblem(metadata, StatusCodes.Status422UnprocessableEntity);
            }

            AddProblem(metadata, StatusCodes.Status500InternalServerError);
        });

        return builder;
    }

    private static void AddProblem(IList<object> metadata, int statusCode)
    {
        if (metadata.OfType<IProducesResponseTypeMetadata>().Any(existing => existing.StatusCode == statusCode))
        {
            return;
        }

        metadata.Add(new ProducesResponseTypeMetadata(statusCode, typeof(ProblemDetails), [_problemJsonContentType]));
    }
}
