using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResultHandler.AspNetCore.Envelopes;
using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Enums;
using ResultHandler.Mapping;

namespace ResultHandler.AspNetCore.Extensions;

public static partial class AspNetCoreResultExtensions
{
    /// <summary>Maps to a Minimal API <see cref="IResult"/> without a body on success. Prefer this over <see cref="ToActionResult(IOperationResult, HttpContext?)"/> in Minimal API delegates — returning <see cref="IActionResult"/> there triggers analyzer warning ASP0004 and is opaque to OpenAPI. <paramref name="httpContext"/>, if given, sets <see cref="ProblemDetails.Instance"/> on failure.</summary>
    public static IResult ToResult(this IOperationResult result, HttpContext? httpContext = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccessful)
        {
            EnsureFinalStatus(result.Status);
            var success = result.Status switch
            {
                ResultStatus.NoContent => Results.NoContent(),
                _ => Results.StatusCode((int)result.Status.ToHttpStatusCode()),
            };
            return WithLocation(result, success);
        }

        return ToProblemResult(result, httpContext);
    }

    /// <summary>Maps to a Minimal API <see cref="IResult"/> whose success body is the raw <typeparamref name="T"/> data (1xx/3xx/NoContent/NotModified carry no body). Prefer this over <see cref="ToActionResult{T}(IOperationResult{T}, HttpContext?)"/> in Minimal API delegates — see <see cref="ToResult(IOperationResult, HttpContext?)"/>.</summary>
    public static IResult ToResult<T>(this IOperationResult<T> result, HttpContext? httpContext = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccessful)
        {
            EnsureFinalStatus(result.Status);
            return WithLocation(result, ToSuccessResult(result.Status, result.Data));
        }

        return ToProblemResult(result, httpContext);
    }

    /// <summary>Maps to a Minimal API <see cref="IResult"/> whose success body is the full result envelope, data + metadata (1xx/3xx/NoContent/NotModified carry no body). Prefer this over <see cref="ToEnvelopedActionResult(IOperationResult, HttpContext?)"/> in Minimal API delegates — see <see cref="ToResult(IOperationResult, HttpContext?)"/>.</summary>
    public static IResult ToEnvelopedResult(this IOperationResult result, HttpContext? httpContext = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccessful)
        {
            EnsureFinalStatus(result.Status);
            var success = ToBodylessSuccessResult(result.Status)
                ?? Results.Json(ResultEnvelopeFactory.Create(result), statusCode: (int)result.Status.ToHttpStatusCode());
            return WithLocation(result, success);
        }

        return ToProblemResult(result, httpContext);
    }

    /// <summary>Maps a failed result to a Minimal API <see cref="IResult"/> carrying RFC 9457 <see cref="ProblemDetails"/> (see <see cref="ToProblemDetails"/>), written through <see cref="IProblemDetailsService"/> when one is registered. The request's instance and trace id are added when the response is written, so <paramref name="httpContext"/> is optional. The returned value implements <see cref="IValueHttpResult{TValue}"/> of <see cref="ProblemDetails"/> and <see cref="IStatusCodeHttpResult"/> for tests.</summary>
    public static IResult ToProblemResult(this IOperationResult result, HttpContext? httpContext = null)
        => new ProblemDetailsHttpResult(result.ToProblemDetails(httpContext));

    private static IResult WithLocation(IOperationResult result, IResult success)
        => result.Location is { } location ? new LocationHttpResult(success, location) : success;

    private static IResult ToSuccessResult<T>(ResultStatus status, T data)
    {
        return ToBodylessSuccessResult(status)
            ?? Results.Json(data, statusCode: (int)status.ToHttpStatusCode());
    }

    private static IResult? ToBodylessSuccessResult(ResultStatus status)
    {
        var (kind, httpCode) = ClassifyBodyless(status);
        return kind switch
        {
            BodylessKind.NoContent => Results.NoContent(),
            BodylessKind.NotModified => Results.StatusCode(304),
            BodylessKind.Generic => Results.StatusCode(httpCode),
            _ => null,
        };
    }
}
