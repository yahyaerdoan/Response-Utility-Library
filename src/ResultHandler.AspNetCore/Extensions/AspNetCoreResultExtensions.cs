using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Enums;
using ResultHandler.Mapping;

namespace ResultHandler.AspNetCore.Extensions;

/// <summary>
/// Converts <see cref="IOperationResult"/>/<see cref="IOperationResult{T}"/> into ASP.NET Core
/// response types. Split across three files: the MVC <see cref="IActionResult"/> surface lives in
/// <c>AspNetCoreResultExtensions.Mvc.cs</c>, the Minimal API <see cref="IResult"/> surface lives in
/// <c>AspNetCoreResultExtensions.MinimalApi.cs</c>, and this file holds what both share — RFC 9457
/// <see cref="ProblemDetails"/> construction and the bodyless-status classification they both need to
/// agree on.
/// </summary>
public static partial class AspNetCoreResultExtensions
{
    // RFC 9110 / RFC 6585 / RFC 4918 / RFC 7725 / RFC 8470 - canonical type URIs per RFC 9457 §4.2
    // internal (not private) so ResultHandler.Tests can assert every ResultStatus is covered.
    internal static readonly Dictionary<ResultStatus, string> _problemTypes =
        new()
        {
            // 1xx, 2xx, 3xx - about:blank per RFC 9457 §4.2.1
            [ResultStatus.Continue] = "about:blank",
            [ResultStatus.SwitchingProtocols] = "about:blank",
            [ResultStatus.Processing] = "about:blank",
            [ResultStatus.EarlyHints] = "about:blank",
            [ResultStatus.Ok] = "about:blank",
            [ResultStatus.Created] = "about:blank",
            [ResultStatus.Accepted] = "about:blank",
            [ResultStatus.NonAuthoritativeInformation] = "about:blank",
            [ResultStatus.NoContent] = "about:blank",
            [ResultStatus.ResetContent] = "about:blank",
            [ResultStatus.PartialContent] = "about:blank",
            [ResultStatus.MultiStatus] = "about:blank",
            [ResultStatus.AlreadyReported] = "about:blank",
            [ResultStatus.ImUsed] = "about:blank",
            [ResultStatus.MultipleChoices] = "about:blank",
            [ResultStatus.MovedPermanently] = "about:blank",
            [ResultStatus.Found] = "about:blank",
            [ResultStatus.SeeOther] = "about:blank",
            [ResultStatus.NotModified] = "about:blank",
            [ResultStatus.UseProxy] = "about:blank",
            [ResultStatus.TemporaryRedirect] = "about:blank",
            [ResultStatus.PermanentRedirect] = "about:blank",

            // 4xx - RFC 9110 §15.5.x
            [ResultStatus.BadRequest] = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            [ResultStatus.Unauthorized] = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
            [ResultStatus.PaymentRequired] = "https://tools.ietf.org/html/rfc9110#section-15.5.3",
            [ResultStatus.Forbidden] = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
            [ResultStatus.NotFound] = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            [ResultStatus.MethodNotAllowed] = "https://tools.ietf.org/html/rfc9110#section-15.5.6",
            [ResultStatus.NotAcceptable] = "https://tools.ietf.org/html/rfc9110#section-15.5.7",
            [ResultStatus.ProxyAuthenticationRequired] = "https://tools.ietf.org/html/rfc9110#section-15.5.8",
            [ResultStatus.RequestTimeout] = "https://tools.ietf.org/html/rfc9110#section-15.5.9",
            [ResultStatus.Conflict] = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
            [ResultStatus.Gone] = "https://tools.ietf.org/html/rfc9110#section-15.5.11",
            [ResultStatus.LengthRequired] = "https://tools.ietf.org/html/rfc9110#section-15.5.12",
            [ResultStatus.PreconditionFailed] = "https://tools.ietf.org/html/rfc9110#section-15.5.13",
            [ResultStatus.ContentTooLarge] = "https://tools.ietf.org/html/rfc9110#section-15.5.14",
            [ResultStatus.UriTooLong] = "https://tools.ietf.org/html/rfc9110#section-15.5.15",
            [ResultStatus.UnsupportedMediaType] = "https://tools.ietf.org/html/rfc9110#section-15.5.16",
            [ResultStatus.RangeNotSatisfiable] = "https://tools.ietf.org/html/rfc9110#section-15.5.17",
            [ResultStatus.ExpectationFailed] = "https://tools.ietf.org/html/rfc9110#section-15.5.18",
            [ResultStatus.ImATeapot] = "https://tools.ietf.org/html/rfc9110#section-15.5.19",
            [ResultStatus.MisdirectedRequest] = "https://tools.ietf.org/html/rfc9110#section-15.5.20",
            [ResultStatus.UnprocessableContent] = "https://tools.ietf.org/html/rfc9110#section-15.5.21",
            [ResultStatus.Locked] = "https://tools.ietf.org/html/rfc4918#section-11.3",
            [ResultStatus.FailedDependency] = "https://tools.ietf.org/html/rfc4918#section-11.4",
            [ResultStatus.TooEarly] = "https://tools.ietf.org/html/rfc8470#section-5.2",
            [ResultStatus.UpgradeRequired] = "https://tools.ietf.org/html/rfc9110#section-15.5.23",
            [ResultStatus.PreconditionRequired] = "https://tools.ietf.org/html/rfc6585#section-3",
            [ResultStatus.TooManyRequests] = "https://tools.ietf.org/html/rfc6585#section-4",
            [ResultStatus.RequestHeaderFieldsTooLarge] = "https://tools.ietf.org/html/rfc6585#section-5",
            [ResultStatus.UnavailableForLegalReasons] = "https://tools.ietf.org/html/rfc7725#section-3",

            // 5xx - RFC 9110 §15.6.x / RFC 4918 / RFC 2295 / RFC 5842 / RFC 2774 / RFC 6585
            [ResultStatus.InternalServerError] = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            [ResultStatus.NotImplemented] = "https://tools.ietf.org/html/rfc9110#section-15.6.2",
            [ResultStatus.BadGateway] = "https://tools.ietf.org/html/rfc9110#section-15.6.3",
            [ResultStatus.ServiceUnavailable] = "https://tools.ietf.org/html/rfc9110#section-15.6.4",
            [ResultStatus.GatewayTimeout] = "https://tools.ietf.org/html/rfc9110#section-15.6.5",
            [ResultStatus.HttpVersionNotSupported] = "https://tools.ietf.org/html/rfc9110#section-15.6.6",
            [ResultStatus.VariantAlsoNegotiates] = "https://tools.ietf.org/html/rfc2295#section-8.1",
            [ResultStatus.InsufficientStorage] = "https://tools.ietf.org/html/rfc4918#section-11.5",
            [ResultStatus.LoopDetected] = "https://tools.ietf.org/html/rfc5842#section-7.2",
            [ResultStatus.NotExtended] = "https://tools.ietf.org/html/rfc2774#section-7",
            [ResultStatus.NetworkAuthenticationRequired] = "https://tools.ietf.org/html/rfc6585#section-6",
        };

    private enum BodylessKind
    {
        None,
        NoContent,
        NotModified,
        Generic,
    }

    /// <summary>Creates an RFC 9457 <see cref="ProblemDetails"/> payload from an <see cref="IOperationResult"/>.</summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="httpContext">
    /// Optional; when provided, <see cref="ProblemDetails.Instance"/> is set to the current request
    /// path, per RFC 9457's guidance that it identify "this specific occurrence" of the problem.
    /// </param>
    public static ProblemDetails ToProblemDetails(this IOperationResult result, HttpContext? httpContext = null)
    {
        var problem = new ProblemDetails
        {
            Type = _problemTypes.TryGetValue(result.Status, out var type) ? type : "about:blank",
            Title = result.Title,
            Detail = result.Detail,
            Status = (int)result.Status.ToHttpStatusCode(),
            Instance = httpContext?.Request.Path.Value,
        };

        if (result.Errors.Count > 0)
        {
            problem.Extensions["errors"] = result.Errors;
        }

        if (result.FieldErrors.Count > 0)
        {
            problem.Extensions["fieldErrors"] = result.FieldErrors;
        }

        return problem;
    }

    /// <summary>
    /// Single source of truth for which statuses map to a bodyless response and what HTTP code to
    /// use — shared by both the <see cref="IActionResult"/> and Minimal API <see cref="IResult"/>
    /// surfaces so the two never drift apart.
    /// </summary>
    private static (BodylessKind Kind, int HttpCode) ClassifyBodyless(ResultStatus status)
    {
        var httpCode = (int)status.ToHttpStatusCode();
        return status switch
        {
            ResultStatus.NoContent => (BodylessKind.NoContent, httpCode),
            ResultStatus.NotModified => (BodylessKind.NotModified, 304),
            _ when httpCode is >= 100 and < 200 or >= 300 and < 400 => (BodylessKind.Generic, httpCode),
            _ => (BodylessKind.None, httpCode),
        };
    }
}
