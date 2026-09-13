using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Enums;
using ResultHandler.Mapping;

namespace ResultHandler.AspNetCore.Extensions;

public static partial class AspNetCoreResultExtensions
{
    /// <summary>Maps to an HTTP response without a body on success — use for endpoints that don't return data (e.g. fire-and-forget commands). When <paramref name="httpContext"/> is provided, a failed result's <see cref="ProblemDetails.Instance"/> is set to the current request path.</summary>
    public static IActionResult ToActionResult(this IOperationResult result, HttpContext? httpContext = null)
    {
        if (result.IsSuccessful)
        {
            return result.Status switch
            {
                ResultStatus.NoContent => new NoContentResult(),
                _ => new StatusCodeResult((int)result.Status.ToHttpStatusCode()),
            };
        }

        return ToProblemActionResult(result, httpContext);
    }

    /// <summary>Maps to an HTTP response whose success body is the raw <typeparamref name="T"/> data. 1xx/3xx/NoContent/NotModified carry no body. When <paramref name="httpContext"/> is provided, a failed result's <see cref="ProblemDetails.Instance"/> is set to the current request path.</summary>
    public static IActionResult ToActionResult<T>(this IOperationResult<T> result, HttpContext? httpContext = null)
    {
        if (result.IsSuccessful)
        {
            return ToSuccessActionResult(result.Status, result.Data);
        }

        return ToProblemActionResult(result, httpContext);
    }

    /// <summary>Maps to an HTTP response whose success body is the full result envelope (data + metadata). 1xx/3xx/NoContent/NotModified carry no body. When <paramref name="httpContext"/> is provided, a failed result's <see cref="ProblemDetails.Instance"/> is set to the current request path.</summary>
    public static IActionResult ToEnvelopedActionResult(this IOperationResult result, HttpContext? httpContext = null)
    {
        if (result.IsSuccessful)
        {
            return (IActionResult?)ToBodylessSuccessActionResult(result.Status)
                ?? new ObjectResult(result) { StatusCode = (int)result.Status.ToHttpStatusCode() };
        }

        return ToProblemActionResult(result, httpContext);
    }

    private static IActionResult ToSuccessActionResult<T>(ResultStatus status, T data)
    {
        return (IActionResult?)ToBodylessSuccessActionResult(status)
            ?? new ObjectResult(data) { StatusCode = (int)status.ToHttpStatusCode() };
    }

    private static StatusCodeResult? ToBodylessSuccessActionResult(ResultStatus status)
    {
        var (kind, httpCode) = ClassifyBodyless(status);
        return kind switch
        {
            BodylessKind.NoContent => new NoContentResult(),
            BodylessKind.NotModified => new StatusCodeResult(304),
            BodylessKind.Generic => new StatusCodeResult(httpCode),
            _ => null,
        };
    }

    private static ObjectResult ToProblemActionResult(IOperationResult result, HttpContext? httpContext)
        => new(result.ToProblemDetails(httpContext))
        {
            StatusCode = (int)result.Status.ToHttpStatusCode(),
        };
}
