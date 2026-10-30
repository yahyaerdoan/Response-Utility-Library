using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ResultHandler.AspNetCore.Extensions;

/// <summary>Minimal API result for a failed operation: adds the request's instance and trace id, then writes through <see cref="TypedResults.Problem(ProblemDetails)"/>, which uses <see cref="IProblemDetailsService"/> when one is registered.</summary>
internal sealed class ProblemDetailsHttpResult(ProblemDetails problemDetails)
    : IResult, IStatusCodeHttpResult, IContentTypeHttpResult, IValueHttpResult, IValueHttpResult<ProblemDetails>
{
    public ProblemDetails Value { get; } = problemDetails;

    public int StatusCode => Value.Status ?? StatusCodes.Status500InternalServerError;

    public string ContentType => "application/problem+json";

    int? IStatusCodeHttpResult.StatusCode => StatusCode;

    object? IValueHttpResult.Value => Value;

    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        AspNetCoreResultExtensions.Enrich(Value, httpContext);
        return TypedResults.Problem(Value).ExecuteAsync(httpContext);
    }
}
