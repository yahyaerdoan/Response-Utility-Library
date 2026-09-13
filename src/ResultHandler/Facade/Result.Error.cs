using ResultHandler.Core.Enums;
using ResultHandler.Implementations.Error;

namespace ResultHandler.Facade;

public static partial class Result
{
    /// <summary>Failure (400 Bad Request).</summary>
    public static ErrorResult BadRequest(string detail)
        => new(ResultTitles.BadRequest, ResultStatus.BadRequest, detail);

    /// <inheritdoc cref="BadRequest(string)"/>
    public static ErrorDataResult<T> BadRequest<T>(string detail)
        => new(ResultTitles.BadRequest, ResultStatus.BadRequest, detail);

    /// <summary>Failure (401 Unauthorized).</summary>
    public static ErrorResult Unauthorized(string detail = FailureMessages.Unauthorized)
        => new(ResultTitles.Unauthorized, ResultStatus.Unauthorized, detail);

    /// <inheritdoc cref="Unauthorized(string)"/>
    public static ErrorDataResult<T> Unauthorized<T>(string detail = FailureMessages.Unauthorized)
        => new(ResultTitles.Unauthorized, ResultStatus.Unauthorized, detail);

    /// <summary>Failure (402 Payment Required).</summary>
    public static ErrorResult PaymentRequired(string detail)
        => new(ResultTitles.PaymentRequired, ResultStatus.PaymentRequired, detail);

    /// <inheritdoc cref="PaymentRequired(string)"/>
    public static ErrorDataResult<T> PaymentRequired<T>(string detail)
        => new(ResultTitles.PaymentRequired, ResultStatus.PaymentRequired, detail);

    /// <summary>Failure (403 Forbidden).</summary>
    public static ErrorResult Forbidden(string detail = FailureMessages.Forbidden)
        => new(ResultTitles.Forbidden, ResultStatus.Forbidden, detail);

    /// <inheritdoc cref="Forbidden(string)"/>
    public static ErrorDataResult<T> Forbidden<T>(string detail = FailureMessages.Forbidden)
        => new(ResultTitles.Forbidden, ResultStatus.Forbidden, detail);

    /// <summary>Failure (404 Not Found).</summary>
    public static ErrorResult NotFound(string detail)
        => new(ResultTitles.NotFound, ResultStatus.NotFound, detail);

    /// <inheritdoc cref="NotFound(string)"/>
    public static ErrorDataResult<T> NotFound<T>(string detail)
        => new(ResultTitles.NotFound, ResultStatus.NotFound, detail);

    /// <summary>Failure (405 Method Not Allowed).</summary>
    public static ErrorResult MethodNotAllowed(string detail = FailureMessages.MethodNotAllowed)
        => new(ResultTitles.MethodNotAllowed, ResultStatus.MethodNotAllowed, detail);

    /// <inheritdoc cref="MethodNotAllowed(string)"/>
    public static ErrorDataResult<T> MethodNotAllowed<T>(string detail = FailureMessages.MethodNotAllowed)
        => new(ResultTitles.MethodNotAllowed, ResultStatus.MethodNotAllowed, detail);

    /// <summary>Failure (406 Not Acceptable).</summary>
    public static ErrorResult NotAcceptable(string detail = FailureMessages.NotAcceptable)
        => new(ResultTitles.NotAcceptable, ResultStatus.NotAcceptable, detail);

    /// <inheritdoc cref="NotAcceptable(string)"/>
    public static ErrorDataResult<T> NotAcceptable<T>(string detail = FailureMessages.NotAcceptable)
        => new(ResultTitles.NotAcceptable, ResultStatus.NotAcceptable, detail);

    /// <summary>Failure (407 Proxy Authentication Required).</summary>
    public static ErrorResult ProxyAuthenticationRequired(string detail = FailureMessages.ProxyAuthenticationRequired)
        => new(ResultTitles.ProxyAuthenticationRequired, ResultStatus.ProxyAuthenticationRequired, detail);

    /// <inheritdoc cref="ProxyAuthenticationRequired(string)"/>
    public static ErrorDataResult<T> ProxyAuthenticationRequired<T>(string detail = FailureMessages.ProxyAuthenticationRequired)
        => new(ResultTitles.ProxyAuthenticationRequired, ResultStatus.ProxyAuthenticationRequired, detail);

    /// <summary>Failure (408 Request Timeout).</summary>
    public static ErrorResult RequestTimeout(string detail = FailureMessages.RequestTimeout)
        => new(ResultTitles.RequestTimeout, ResultStatus.RequestTimeout, detail);

    /// <inheritdoc cref="RequestTimeout(string)"/>
    public static ErrorDataResult<T> RequestTimeout<T>(string detail = FailureMessages.RequestTimeout)
        => new(ResultTitles.RequestTimeout, ResultStatus.RequestTimeout, detail);

    /// <summary>Failure (409 Conflict).</summary>
    public static ErrorResult Conflict(string detail)
        => new(ResultTitles.Conflict, ResultStatus.Conflict, detail);

    /// <inheritdoc cref="Conflict(string)"/>
    public static ErrorDataResult<T> Conflict<T>(string detail)
        => new(ResultTitles.Conflict, ResultStatus.Conflict, detail);

    /// <summary>Failure (410 Gone).</summary>
    public static ErrorResult Gone(string detail)
        => new(ResultTitles.Gone, ResultStatus.Gone, detail);

    /// <inheritdoc cref="Gone(string)"/>
    public static ErrorDataResult<T> Gone<T>(string detail)
        => new(ResultTitles.Gone, ResultStatus.Gone, detail);

    /// <summary>Failure (411 Length Required).</summary>
    public static ErrorResult LengthRequired(string detail = FailureMessages.LengthRequired)
        => new(ResultTitles.LengthRequired, ResultStatus.LengthRequired, detail);

    /// <inheritdoc cref="LengthRequired(string)"/>
    public static ErrorDataResult<T> LengthRequired<T>(string detail = FailureMessages.LengthRequired)
        => new(ResultTitles.LengthRequired, ResultStatus.LengthRequired, detail);

    /// <summary>Failure (412 Precondition Failed).</summary>
    public static ErrorResult PreconditionFailed(string detail)
        => new(ResultTitles.PreconditionFailed, ResultStatus.PreconditionFailed, detail);

    /// <inheritdoc cref="PreconditionFailed(string)"/>
    public static ErrorDataResult<T> PreconditionFailed<T>(string detail)
        => new(ResultTitles.PreconditionFailed, ResultStatus.PreconditionFailed, detail);

    /// <summary>Failure (413 Content Too Large).</summary>
    public static ErrorResult ContentTooLarge(string detail)
        => new(ResultTitles.ContentTooLarge, ResultStatus.ContentTooLarge, detail);

    /// <inheritdoc cref="ContentTooLarge(string)"/>
    public static ErrorDataResult<T> ContentTooLarge<T>(string detail)
        => new(ResultTitles.ContentTooLarge, ResultStatus.ContentTooLarge, detail);

    /// <summary>Failure (414 URI Too Long).</summary>
    public static ErrorResult UriTooLong(string detail = FailureMessages.UriTooLong)
        => new(ResultTitles.UriTooLong, ResultStatus.UriTooLong, detail);

    /// <inheritdoc cref="UriTooLong(string)"/>
    public static ErrorDataResult<T> UriTooLong<T>(string detail = FailureMessages.UriTooLong)
        => new(ResultTitles.UriTooLong, ResultStatus.UriTooLong, detail);

    /// <summary>Failure (415 Unsupported Media Type).</summary>
    public static ErrorResult UnsupportedMediaType(string detail = FailureMessages.UnsupportedMediaType)
        => new(ResultTitles.UnsupportedMediaType, ResultStatus.UnsupportedMediaType, detail);

    /// <inheritdoc cref="UnsupportedMediaType(string)"/>
    public static ErrorDataResult<T> UnsupportedMediaType<T>(string detail = FailureMessages.UnsupportedMediaType)
        => new(ResultTitles.UnsupportedMediaType, ResultStatus.UnsupportedMediaType, detail);

    /// <summary>Failure (416 Range Not Satisfiable).</summary>
    public static ErrorResult RangeNotSatisfiable(string detail)
        => new(ResultTitles.RangeNotSatisfiable, ResultStatus.RangeNotSatisfiable, detail);

    /// <inheritdoc cref="RangeNotSatisfiable(string)"/>
    public static ErrorDataResult<T> RangeNotSatisfiable<T>(string detail)
        => new(ResultTitles.RangeNotSatisfiable, ResultStatus.RangeNotSatisfiable, detail);

    /// <summary>Failure (417 Expectation Failed).</summary>
    public static ErrorResult ExpectationFailed(string detail = FailureMessages.ExpectationFailed)
        => new(ResultTitles.ExpectationFailed, ResultStatus.ExpectationFailed, detail);

    /// <inheritdoc cref="ExpectationFailed(string)"/>
    public static ErrorDataResult<T> ExpectationFailed<T>(string detail = FailureMessages.ExpectationFailed)
        => new(ResultTitles.ExpectationFailed, ResultStatus.ExpectationFailed, detail);

    /// <summary>Failure (418 I'm a Teapot).</summary>
    public static ErrorResult ImATeapot(string detail = FailureMessages.ImATeapot)
        => new(ResultTitles.ImATeapot, ResultStatus.ImATeapot, detail);

    /// <inheritdoc cref="ImATeapot(string)"/>
    public static ErrorDataResult<T> ImATeapot<T>(string detail = FailureMessages.ImATeapot)
        => new(ResultTitles.ImATeapot, ResultStatus.ImATeapot, detail);

    /// <summary>Failure (421 Misdirected Request).</summary>
    public static ErrorResult MisdirectedRequest(string detail = FailureMessages.MisdirectedRequest)
        => new(ResultTitles.MisdirectedRequest, ResultStatus.MisdirectedRequest, detail);

    /// <inheritdoc cref="MisdirectedRequest(string)"/>
    public static ErrorDataResult<T> MisdirectedRequest<T>(string detail = FailureMessages.MisdirectedRequest)
        => new(ResultTitles.MisdirectedRequest, ResultStatus.MisdirectedRequest, detail);

    /// <summary>Failure (422 Unprocessable Content).</summary>
    public static ErrorResult UnprocessableContent(string detail)
        => new(ResultTitles.UnprocessableContent, ResultStatus.UnprocessableContent, detail);

    /// <inheritdoc cref="UnprocessableContent(string)"/>
    public static ErrorDataResult<T> UnprocessableContent<T>(string detail)
        => new(ResultTitles.UnprocessableContent, ResultStatus.UnprocessableContent, detail);

    /// <summary>Validation failure (422 Unprocessable Content) from a flat list of error messages.</summary>
    public static ErrorResult Invalid(params string[] errors)
        => new(ResultTitles.ValidationFailed, ResultStatus.UnprocessableContent, (IReadOnlyList<string>)errors);

    /// <inheritdoc cref="Invalid(string[])"/>
    public static ErrorResult Invalid(IReadOnlyList<string> errors)
        => new(ResultTitles.ValidationFailed, ResultStatus.UnprocessableContent, errors);

    /// <inheritdoc cref="Invalid(string[])"/>
    public static ErrorDataResult<T> Invalid<T>(params string[] errors)
        => new(ResultTitles.ValidationFailed, ResultStatus.UnprocessableContent, errors);

    /// <inheritdoc cref="Invalid(string[])"/>
    public static ErrorDataResult<T> Invalid<T>(IReadOnlyList<string> errors)
        => new(ResultTitles.ValidationFailed, ResultStatus.UnprocessableContent, errors);

    /// <summary>Failure (423 Locked).</summary>
    public static ErrorResult Locked(string detail = FailureMessages.Locked)
        => new(ResultTitles.Locked, ResultStatus.Locked, detail);

    /// <inheritdoc cref="Locked(string)"/>
    public static ErrorDataResult<T> Locked<T>(string detail = FailureMessages.Locked)
        => new(ResultTitles.Locked, ResultStatus.Locked, detail);

    /// <summary>Failure (424 Failed Dependency).</summary>
    public static ErrorResult FailedDependency(string detail)
        => new(ResultTitles.FailedDependency, ResultStatus.FailedDependency, detail);

    /// <inheritdoc cref="FailedDependency(string)"/>
    public static ErrorDataResult<T> FailedDependency<T>(string detail)
        => new(ResultTitles.FailedDependency, ResultStatus.FailedDependency, detail);

    /// <summary>Failure (425 Too Early).</summary>
    public static ErrorResult TooEarly(string detail = FailureMessages.TooEarly)
        => new(ResultTitles.TooEarly, ResultStatus.TooEarly, detail);

    /// <inheritdoc cref="TooEarly(string)"/>
    public static ErrorDataResult<T> TooEarly<T>(string detail = FailureMessages.TooEarly)
        => new(ResultTitles.TooEarly, ResultStatus.TooEarly, detail);

    /// <summary>Failure (426 Upgrade Required).</summary>
    public static ErrorResult UpgradeRequired(string detail)
        => new(ResultTitles.UpgradeRequired, ResultStatus.UpgradeRequired, detail);

    /// <inheritdoc cref="UpgradeRequired(string)"/>
    public static ErrorDataResult<T> UpgradeRequired<T>(string detail)
        => new(ResultTitles.UpgradeRequired, ResultStatus.UpgradeRequired, detail);

    /// <summary>Failure (428 Precondition Required).</summary>
    public static ErrorResult PreconditionRequired(string detail)
        => new(ResultTitles.PreconditionRequired, ResultStatus.PreconditionRequired, detail);

    /// <inheritdoc cref="PreconditionRequired(string)"/>
    public static ErrorDataResult<T> PreconditionRequired<T>(string detail)
        => new(ResultTitles.PreconditionRequired, ResultStatus.PreconditionRequired, detail);

    /// <summary>Failure (429 Too Many Requests).</summary>
    public static ErrorResult TooManyRequests(string detail = FailureMessages.TooManyRequests)
        => new(ResultTitles.TooManyRequests, ResultStatus.TooManyRequests, detail);

    /// <inheritdoc cref="TooManyRequests(string)"/>
    public static ErrorDataResult<T> TooManyRequests<T>(string detail = FailureMessages.TooManyRequests)
        => new(ResultTitles.TooManyRequests, ResultStatus.TooManyRequests, detail);

    /// <summary>Failure (431 Request Header Fields Too Large).</summary>
    public static ErrorResult RequestHeaderFieldsTooLarge(string detail = FailureMessages.RequestHeaderFieldsTooLarge)
        => new(ResultTitles.RequestHeaderFieldsTooLarge, ResultStatus.RequestHeaderFieldsTooLarge, detail);

    /// <inheritdoc cref="RequestHeaderFieldsTooLarge(string)"/>
    public static ErrorDataResult<T> RequestHeaderFieldsTooLarge<T>(string detail = FailureMessages.RequestHeaderFieldsTooLarge)
        => new(ResultTitles.RequestHeaderFieldsTooLarge, ResultStatus.RequestHeaderFieldsTooLarge, detail);

    /// <summary>Failure (451 Unavailable For Legal Reasons).</summary>
    public static ErrorResult UnavailableForLegalReasons(string detail)
        => new(ResultTitles.UnavailableForLegalReasons, ResultStatus.UnavailableForLegalReasons, detail);

    /// <inheritdoc cref="UnavailableForLegalReasons(string)"/>
    public static ErrorDataResult<T> UnavailableForLegalReasons<T>(string detail)
        => new(ResultTitles.UnavailableForLegalReasons, ResultStatus.UnavailableForLegalReasons, detail);

    /// <summary>Failure (500 Internal Server Error).</summary>
    public static ErrorResult InternalServerError(string detail = FailureMessages.InternalServerError)
        => new(ResultTitles.InternalServerError, ResultStatus.InternalServerError, detail);

    /// <inheritdoc cref="InternalServerError(string)"/>
    public static ErrorDataResult<T> InternalServerError<T>(string detail = FailureMessages.InternalServerError)
        => new(ResultTitles.InternalServerError, ResultStatus.InternalServerError, detail);

    /// <summary>Failure (501 Not Implemented).</summary>
    public static ErrorResult NotImplemented(string detail = FailureMessages.NotImplemented)
        => new(ResultTitles.NotImplemented, ResultStatus.NotImplemented, detail);

    /// <inheritdoc cref="NotImplemented(string)"/>
    public static ErrorDataResult<T> NotImplemented<T>(string detail = FailureMessages.NotImplemented)
        => new(ResultTitles.NotImplemented, ResultStatus.NotImplemented, detail);

    /// <summary>Failure (502 Bad Gateway).</summary>
    public static ErrorResult BadGateway(string detail = FailureMessages.BadGateway)
        => new(ResultTitles.BadGateway, ResultStatus.BadGateway, detail);

    /// <inheritdoc cref="BadGateway(string)"/>
    public static ErrorDataResult<T> BadGateway<T>(string detail = FailureMessages.BadGateway)
        => new(ResultTitles.BadGateway, ResultStatus.BadGateway, detail);

    /// <summary>Failure (503 Service Unavailable).</summary>
    public static ErrorResult ServiceUnavailable(string detail = FailureMessages.ServiceUnavailable)
        => new(ResultTitles.ServiceUnavailable, ResultStatus.ServiceUnavailable, detail);

    /// <inheritdoc cref="ServiceUnavailable(string)"/>
    public static ErrorDataResult<T> ServiceUnavailable<T>(string detail = FailureMessages.ServiceUnavailable)
        => new(ResultTitles.ServiceUnavailable, ResultStatus.ServiceUnavailable, detail);

    /// <summary>Failure (504 Gateway Timeout).</summary>
    public static ErrorResult GatewayTimeout(string detail = FailureMessages.GatewayTimeout)
        => new(ResultTitles.GatewayTimeout, ResultStatus.GatewayTimeout, detail);

    /// <inheritdoc cref="GatewayTimeout(string)"/>
    public static ErrorDataResult<T> GatewayTimeout<T>(string detail = FailureMessages.GatewayTimeout)
        => new(ResultTitles.GatewayTimeout, ResultStatus.GatewayTimeout, detail);

    /// <summary>Failure (505 HTTP Version Not Supported).</summary>
    public static ErrorResult HttpVersionNotSupported(string detail = FailureMessages.HttpVersionNotSupported)
        => new(ResultTitles.HttpVersionNotSupported, ResultStatus.HttpVersionNotSupported, detail);

    /// <inheritdoc cref="HttpVersionNotSupported(string)"/>
    public static ErrorDataResult<T> HttpVersionNotSupported<T>(string detail = FailureMessages.HttpVersionNotSupported)
        => new(ResultTitles.HttpVersionNotSupported, ResultStatus.HttpVersionNotSupported, detail);

    /// <summary>Failure (506 Variant Also Negotiates).</summary>
    public static ErrorResult VariantAlsoNegotiates(string detail = FailureMessages.VariantAlsoNegotiates)
        => new(ResultTitles.VariantAlsoNegotiates, ResultStatus.VariantAlsoNegotiates, detail);

    /// <inheritdoc cref="VariantAlsoNegotiates(string)"/>
    public static ErrorDataResult<T> VariantAlsoNegotiates<T>(string detail = FailureMessages.VariantAlsoNegotiates)
        => new(ResultTitles.VariantAlsoNegotiates, ResultStatus.VariantAlsoNegotiates, detail);

    /// <summary>Failure (507 Insufficient Storage).</summary>
    public static ErrorResult InsufficientStorage(string detail = FailureMessages.InsufficientStorage)
        => new(ResultTitles.InsufficientStorage, ResultStatus.InsufficientStorage, detail);

    /// <inheritdoc cref="InsufficientStorage(string)"/>
    public static ErrorDataResult<T> InsufficientStorage<T>(string detail = FailureMessages.InsufficientStorage)
        => new(ResultTitles.InsufficientStorage, ResultStatus.InsufficientStorage, detail);

    /// <summary>Failure (508 Loop Detected).</summary>
    public static ErrorResult LoopDetected(string detail = FailureMessages.LoopDetected)
        => new(ResultTitles.LoopDetected, ResultStatus.LoopDetected, detail);

    /// <inheritdoc cref="LoopDetected(string)"/>
    public static ErrorDataResult<T> LoopDetected<T>(string detail = FailureMessages.LoopDetected)
        => new(ResultTitles.LoopDetected, ResultStatus.LoopDetected, detail);

    /// <summary>Failure (510 Not Extended).</summary>
    public static ErrorResult NotExtended(string detail = FailureMessages.NotExtended)
        => new(ResultTitles.NotExtended, ResultStatus.NotExtended, detail);

    /// <inheritdoc cref="NotExtended(string)"/>
    public static ErrorDataResult<T> NotExtended<T>(string detail = FailureMessages.NotExtended)
        => new(ResultTitles.NotExtended, ResultStatus.NotExtended, detail);

    /// <summary>Failure (511 Network Authentication Required).</summary>
    public static ErrorResult NetworkAuthenticationRequired(string detail = FailureMessages.NetworkAuthenticationRequired)
        => new(ResultTitles.NetworkAuthenticationRequired, ResultStatus.NetworkAuthenticationRequired, detail);

    /// <inheritdoc cref="NetworkAuthenticationRequired(string)"/>
    public static ErrorDataResult<T> NetworkAuthenticationRequired<T>(string detail = FailureMessages.NetworkAuthenticationRequired)
        => new(ResultTitles.NetworkAuthenticationRequired, ResultStatus.NetworkAuthenticationRequired, detail);

    /// <summary>Escape hatch: a failure with a custom title, detail, and status, for anything not covered by a named factory.</summary>
    public static ErrorResult Failure(string title, string detail, ResultStatus status)
        => new(title, status, detail);

    /// <inheritdoc cref="Failure(string, string, ResultStatus)"/>
    public static ErrorDataResult<T> Failure<T>(string title, string detail, ResultStatus status)
        => new(title, status, detail);

    /// <inheritdoc cref="Failure(string, string, ResultStatus)"/>
    public static ErrorDataResult<T> Failure<T>(T data, string title, string detail, ResultStatus status)
        => new(data, title, status, detail);
}
