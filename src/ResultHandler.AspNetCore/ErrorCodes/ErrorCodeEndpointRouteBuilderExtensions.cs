using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ResultHandler.AspNetCore.Exceptions;
using ResultHandler.AspNetCore.Extensions;
using ResultHandler.Facade;

namespace ResultHandler.AspNetCore.ErrorCodes;

/// <summary>Serves the error code catalog.</summary>
public static class ErrorCodeEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps <c>GET {path}</c>, every code (<c>"ListErrorCodes"</c>), and <c>GET {path}/{code}</c>, one code (<c>"GetErrorCode"</c>; an unknown code
    /// is a 404 with <see cref="ResultErrorCodes.UnknownErrorCode"/>), at <see cref="ErrorCodeCatalogOptions.Path"/>. Requires
    /// <c>AddErrorCodeCatalog()</c>. Returns the route group, for conventions such as API version neutrality.
    /// </summary>
    public static RouteGroupBuilder MapErrorCodes(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var path = endpoints.ServiceProvider.GetRequiredService<IOptions<ErrorCodeCatalogOptions>>().Value.Path;
        var group = endpoints.MapGroup(path).WithTags("Errors");

        _ = group.MapGet(string.Empty, (ErrorCodeCatalog catalog) => TypedResults.Ok(catalog.All))
            .WithName("ListErrorCodes")
            .WithDescription("Every code a failed response can carry in \"code\", with the status it comes with and what it means.")
            .Produces<IReadOnlyCollection<ErrorCodeDescription>>();

        _ = group.MapGet("{code}", (string code, ErrorCodeCatalog catalog, HttpContext httpContext) => catalog.Find(code) is { } description
                ? Results.Ok(description)
                : Result.NotFound($"There is no error code '{code}'.").WithCode(ResultErrorCodes.UnknownErrorCode).ToResult(httpContext))
            .WithName("GetErrorCode")
            .WithDescription("Describes one code from a failed response's \"code\".")
            .Produces<ErrorCodeDescription>()
            .ProducesResultProblem(StatusCodes.Status404NotFound);

        return group;
    }
}
