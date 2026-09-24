using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;
using ResultHandler.Implementations.Error;

namespace ResultHandler.Facade;

public static partial class Result
{
    /// <summary>Failure (400 Bad Request).</summary>
    public static OperationResult BadRequest(string detail)
        => new ErrorResult(ResultTitles.BadRequest, ResultStatus.BadRequest, detail);

    /// <inheritdoc cref="BadRequest(string)"/>
    public static OperationDataResult<T> BadRequest<T>(string detail)
        => new ErrorDataResult<T>(ResultTitles.BadRequest, ResultStatus.BadRequest, detail);

    /// <summary>Failure (401 Unauthorized).</summary>
    public static OperationResult Unauthorized(string detail = FailureMessages.Unauthorized)
        => new ErrorResult(ResultTitles.Unauthorized, ResultStatus.Unauthorized, detail);

    /// <inheritdoc cref="Unauthorized(string)"/>
    public static OperationDataResult<T> Unauthorized<T>(string detail = FailureMessages.Unauthorized)
        => new ErrorDataResult<T>(ResultTitles.Unauthorized, ResultStatus.Unauthorized, detail);

    /// <summary>Failure (402 Payment Required).</summary>
    public static OperationResult PaymentRequired(string detail)
        => new ErrorResult(ResultTitles.PaymentRequired, ResultStatus.PaymentRequired, detail);

    /// <inheritdoc cref="PaymentRequired(string)"/>
    public static OperationDataResult<T> PaymentRequired<T>(string detail)
        => new ErrorDataResult<T>(ResultTitles.PaymentRequired, ResultStatus.PaymentRequired, detail);

    /// <summary>Failure (403 Forbidden).</summary>
    public static OperationResult Forbidden(string detail = FailureMessages.Forbidden)
        => new ErrorResult(ResultTitles.Forbidden, ResultStatus.Forbidden, detail);

    /// <inheritdoc cref="Forbidden(string)"/>
    public static OperationDataResult<T> Forbidden<T>(string detail = FailureMessages.Forbidden)
        => new ErrorDataResult<T>(ResultTitles.Forbidden, ResultStatus.Forbidden, detail);

    /// <summary>Failure (404 Not Found).</summary>
    public static OperationResult NotFound(string detail)
        => new ErrorResult(ResultTitles.NotFound, ResultStatus.NotFound, detail);

    /// <inheritdoc cref="NotFound(string)"/>
    public static OperationDataResult<T> NotFound<T>(string detail)
        => new ErrorDataResult<T>(ResultTitles.NotFound, ResultStatus.NotFound, detail);

    /// <summary>Failure (405 Method Not Allowed).</summary>
    public static OperationResult MethodNotAllowed(string detail = FailureMessages.MethodNotAllowed)
        => new ErrorResult(ResultTitles.MethodNotAllowed, ResultStatus.MethodNotAllowed, detail);

    /// <inheritdoc cref="MethodNotAllowed(string)"/>
    public static OperationDataResult<T> MethodNotAllowed<T>(string detail = FailureMessages.MethodNotAllowed)
        => new ErrorDataResult<T>(ResultTitles.MethodNotAllowed, ResultStatus.MethodNotAllowed, detail);

    /// <summary>Failure (406 Not Acceptable).</summary>
    public static OperationResult NotAcceptable(string detail = FailureMessages.NotAcceptable)
        => new ErrorResult(ResultTitles.NotAcceptable, ResultStatus.NotAcceptable, detail);

    /// <inheritdoc cref="NotAcceptable(string)"/>
    public static OperationDataResult<T> NotAcceptable<T>(string detail = FailureMessages.NotAcceptable)
        => new ErrorDataResult<T>(ResultTitles.NotAcceptable, ResultStatus.NotAcceptable, detail);

    /// <summary>Failure (407 Proxy Authentication Required).</summary>
    public static OperationResult ProxyAuthenticationRequired(string detail = FailureMessages.ProxyAuthenticationRequired)
        => new ErrorResult(ResultTitles.ProxyAuthenticationRequired, ResultStatus.ProxyAuthenticationRequired, detail);

    /// <inheritdoc cref="ProxyAuthenticationRequired(string)"/>
    public static OperationDataResult<T> ProxyAuthenticationRequired<T>(string detail = FailureMessages.ProxyAuthenticationRequired)
        => new ErrorDataResult<T>(ResultTitles.ProxyAuthenticationRequired, ResultStatus.ProxyAuthenticationRequired, detail);

    /// <summary>Failure (408 Request Timeout).</summary>
    public static OperationResult RequestTimeout(string detail = FailureMessages.RequestTimeout)
        => new ErrorResult(ResultTitles.RequestTimeout, ResultStatus.RequestTimeout, detail);

    /// <inheritdoc cref="RequestTimeout(string)"/>
    public static OperationDataResult<T> RequestTimeout<T>(string detail = FailureMessages.RequestTimeout)
        => new ErrorDataResult<T>(ResultTitles.RequestTimeout, ResultStatus.RequestTimeout, detail);

    /// <summary>Failure (409 Conflict).</summary>
    public static OperationResult Conflict(string detail)
        => new ErrorResult(ResultTitles.Conflict, ResultStatus.Conflict, detail);

    /// <inheritdoc cref="Conflict(string)"/>
    public static OperationDataResult<T> Conflict<T>(string detail)
        => new ErrorDataResult<T>(ResultTitles.Conflict, ResultStatus.Conflict, detail);

    /// <summary>Failure (410 Gone).</summary>
    public static OperationResult Gone(string detail)
        => new ErrorResult(ResultTitles.Gone, ResultStatus.Gone, detail);

    /// <inheritdoc cref="Gone(string)"/>
    public static OperationDataResult<T> Gone<T>(string detail)
        => new ErrorDataResult<T>(ResultTitles.Gone, ResultStatus.Gone, detail);

    /// <summary>Failure (411 Length Required).</summary>
    public static OperationResult LengthRequired(string detail = FailureMessages.LengthRequired)
        => new ErrorResult(ResultTitles.LengthRequired, ResultStatus.LengthRequired, detail);

    /// <inheritdoc cref="LengthRequired(string)"/>
    public static OperationDataResult<T> LengthRequired<T>(string detail = FailureMessages.LengthRequired)
        => new ErrorDataResult<T>(ResultTitles.LengthRequired, ResultStatus.LengthRequired, detail);

    /// <summary>Failure (412 Precondition Failed).</summary>
    public static OperationResult PreconditionFailed(string detail)
        => new ErrorResult(ResultTitles.PreconditionFailed, ResultStatus.PreconditionFailed, detail);

    /// <inheritdoc cref="PreconditionFailed(string)"/>
    public static OperationDataResult<T> PreconditionFailed<T>(string detail)
        => new ErrorDataResult<T>(ResultTitles.PreconditionFailed, ResultStatus.PreconditionFailed, detail);

    /// <summary>Failure (413 Content Too Large).</summary>
    public static OperationResult ContentTooLarge(string detail)
        => new ErrorResult(ResultTitles.ContentTooLarge, ResultStatus.ContentTooLarge, detail);

    /// <inheritdoc cref="ContentTooLarge(string)"/>
    public static OperationDataResult<T> ContentTooLarge<T>(string detail)
        => new ErrorDataResult<T>(ResultTitles.ContentTooLarge, ResultStatus.ContentTooLarge, detail);

    /// <summary>Failure (414 URI Too Long).</summary>
    public static OperationResult UriTooLong(string detail = FailureMessages.UriTooLong)
        => new ErrorResult(ResultTitles.UriTooLong, ResultStatus.UriTooLong, detail);

    /// <inheritdoc cref="UriTooLong(string)"/>
    public static OperationDataResult<T> UriTooLong<T>(string detail = FailureMessages.UriTooLong)
        => new ErrorDataResult<T>(ResultTitles.UriTooLong, ResultStatus.UriTooLong, detail);

    /// <summary>Failure (415 Unsupported Media Type).</summary>
    public static OperationResult UnsupportedMediaType(string detail = FailureMessages.UnsupportedMediaType)
        => new ErrorResult(ResultTitles.UnsupportedMediaType, ResultStatus.UnsupportedMediaType, detail);

    /// <inheritdoc cref="UnsupportedMediaType(string)"/>
    public static OperationDataResult<T> UnsupportedMediaType<T>(string detail = FailureMessages.UnsupportedMediaType)
        => new ErrorDataResult<T>(ResultTitles.UnsupportedMediaType, ResultStatus.UnsupportedMediaType, detail);

    /// <summary>Failure (416 Range Not Satisfiable).</summary>
    public static OperationResult RangeNotSatisfiable(string detail)
        => new ErrorResult(ResultTitles.RangeNotSatisfiable, ResultStatus.RangeNotSatisfiable, detail);

    /// <inheritdoc cref="RangeNotSatisfiable(string)"/>
    public static OperationDataResult<T> RangeNotSatisfiable<T>(string detail)
        => new ErrorDataResult<T>(ResultTitles.RangeNotSatisfiable, ResultStatus.RangeNotSatisfiable, detail);

    /// <summary>Failure (417 Expectation Failed).</summary>
    public static OperationResult ExpectationFailed(string detail = FailureMessages.ExpectationFailed)
        => new ErrorResult(ResultTitles.ExpectationFailed, ResultStatus.ExpectationFailed, detail);

    /// <inheritdoc cref="ExpectationFailed(string)"/>
    public static OperationDataResult<T> ExpectationFailed<T>(string detail = FailureMessages.ExpectationFailed)
        => new ErrorDataResult<T>(ResultTitles.ExpectationFailed, ResultStatus.ExpectationFailed, detail);

    /// <summary>Failure (418 I'm a Teapot).</summary>
    public static OperationResult ImATeapot(string detail = FailureMessages.ImATeapot)
        => new ErrorResult(ResultTitles.ImATeapot, ResultStatus.ImATeapot, detail);

    /// <inheritdoc cref="ImATeapot(string)"/>
    public static OperationDataResult<T> ImATeapot<T>(string detail = FailureMessages.ImATeapot)
        => new ErrorDataResult<T>(ResultTitles.ImATeapot, ResultStatus.ImATeapot, detail);

    /// <summary>Failure (421 Misdirected Request).</summary>
    public static OperationResult MisdirectedRequest(string detail = FailureMessages.MisdirectedRequest)
        => new ErrorResult(ResultTitles.MisdirectedRequest, ResultStatus.MisdirectedRequest, detail);

    /// <inheritdoc cref="MisdirectedRequest(string)"/>
    public static OperationDataResult<T> MisdirectedRequest<T>(string detail = FailureMessages.MisdirectedRequest)
        => new ErrorDataResult<T>(ResultTitles.MisdirectedRequest, ResultStatus.MisdirectedRequest, detail);

    /// <summary>Failure (422 Unprocessable Content).</summary>
    public static OperationResult UnprocessableContent(string detail)
        => new ErrorResult(ResultTitles.UnprocessableContent, ResultStatus.UnprocessableContent, detail);

    /// <inheritdoc cref="UnprocessableContent(string)"/>
    public static OperationDataResult<T> UnprocessableContent<T>(string detail)
        => new ErrorDataResult<T>(ResultTitles.UnprocessableContent, ResultStatus.UnprocessableContent, detail);

    /// <summary>Validation failure (422 Unprocessable Content) from a flat list of error messages.</summary>
    public static OperationResult Invalid(params string[] errors)
        => new ErrorResult(ResultTitles.ValidationFailed, ResultStatus.UnprocessableContent, (IReadOnlyList<string>)errors);

    /// <inheritdoc cref="Invalid(string[])"/>
    public static OperationResult Invalid(IReadOnlyList<string> errors)
        => new ErrorResult(ResultTitles.ValidationFailed, ResultStatus.UnprocessableContent, errors);

    /// <inheritdoc cref="Invalid(string[])"/>
    public static OperationDataResult<T> Invalid<T>(params string[] errors)
        => new ErrorDataResult<T>(ResultTitles.ValidationFailed, ResultStatus.UnprocessableContent, errors);

    /// <inheritdoc cref="Invalid(string[])"/>
    public static OperationDataResult<T> Invalid<T>(IReadOnlyList<string> errors)
        => new ErrorDataResult<T>(ResultTitles.ValidationFailed, ResultStatus.UnprocessableContent, errors);

    /// <summary>Failure (423 Locked).</summary>
    public static OperationResult Locked(string detail = FailureMessages.Locked)
        => new ErrorResult(ResultTitles.Locked, ResultStatus.Locked, detail);

    /// <inheritdoc cref="Locked(string)"/>
    public static OperationDataResult<T> Locked<T>(string detail = FailureMessages.Locked)
        => new ErrorDataResult<T>(ResultTitles.Locked, ResultStatus.Locked, detail);

    /// <summary>Failure (424 Failed Dependency).</summary>
    public static OperationResult FailedDependency(string detail)
        => new ErrorResult(ResultTitles.FailedDependency, ResultStatus.FailedDependency, detail);

    /// <inheritdoc cref="FailedDependency(string)"/>
    public static OperationDataResult<T> FailedDependency<T>(string detail)
        => new ErrorDataResult<T>(ResultTitles.FailedDependency, ResultStatus.FailedDependency, detail);

    /// <summary>Failure (425 Too Early).</summary>
    public static OperationResult TooEarly(string detail = FailureMessages.TooEarly)
        => new ErrorResult(ResultTitles.TooEarly, ResultStatus.TooEarly, detail);

    /// <inheritdoc cref="TooEarly(string)"/>
    public static OperationDataResult<T> TooEarly<T>(string detail = FailureMessages.TooEarly)
        => new ErrorDataResult<T>(ResultTitles.TooEarly, ResultStatus.TooEarly, detail);

    /// <summary>Failure (426 Upgrade Required).</summary>
    public static OperationResult UpgradeRequired(string detail)
        => new ErrorResult(ResultTitles.UpgradeRequired, ResultStatus.UpgradeRequired, detail);

    /// <inheritdoc cref="UpgradeRequired(string)"/>
    public static OperationDataResult<T> UpgradeRequired<T>(string detail)
        => new ErrorDataResult<T>(ResultTitles.UpgradeRequired, ResultStatus.UpgradeRequired, detail);

    /// <summary>Failure (428 Precondition Required).</summary>
    public static OperationResult PreconditionRequired(string detail)
        => new ErrorResult(ResultTitles.PreconditionRequired, ResultStatus.PreconditionRequired, detail);

    /// <inheritdoc cref="PreconditionRequired(string)"/>
    public static OperationDataResult<T> PreconditionRequired<T>(string detail)
        => new ErrorDataResult<T>(ResultTitles.PreconditionRequired, ResultStatus.PreconditionRequired, detail);

    /// <summary>Failure (429 Too Many Requests).</summary>
    public static OperationResult TooManyRequests(string detail = FailureMessages.TooManyRequests)
        => new ErrorResult(ResultTitles.TooManyRequests, ResultStatus.TooManyRequests, detail);

    /// <inheritdoc cref="TooManyRequests(string)"/>
    public static OperationDataResult<T> TooManyRequests<T>(string detail = FailureMessages.TooManyRequests)
        => new ErrorDataResult<T>(ResultTitles.TooManyRequests, ResultStatus.TooManyRequests, detail);

    /// <summary>Failure (431 Request Header Fields Too Large).</summary>
    public static OperationResult RequestHeaderFieldsTooLarge(string detail = FailureMessages.RequestHeaderFieldsTooLarge)
        => new ErrorResult(ResultTitles.RequestHeaderFieldsTooLarge, ResultStatus.RequestHeaderFieldsTooLarge, detail);

    /// <inheritdoc cref="RequestHeaderFieldsTooLarge(string)"/>
    public static OperationDataResult<T> RequestHeaderFieldsTooLarge<T>(string detail = FailureMessages.RequestHeaderFieldsTooLarge)
        => new ErrorDataResult<T>(ResultTitles.RequestHeaderFieldsTooLarge, ResultStatus.RequestHeaderFieldsTooLarge, detail);

    /// <summary>Failure (451 Unavailable For Legal Reasons).</summary>
    public static OperationResult UnavailableForLegalReasons(string detail)
        => new ErrorResult(ResultTitles.UnavailableForLegalReasons, ResultStatus.UnavailableForLegalReasons, detail);

    /// <inheritdoc cref="UnavailableForLegalReasons(string)"/>
    public static OperationDataResult<T> UnavailableForLegalReasons<T>(string detail)
        => new ErrorDataResult<T>(ResultTitles.UnavailableForLegalReasons, ResultStatus.UnavailableForLegalReasons, detail);

    /// <summary>Failure (500 Internal Server Error).</summary>
    public static OperationResult InternalServerError(string detail = FailureMessages.InternalServerError)
        => new ErrorResult(ResultTitles.InternalServerError, ResultStatus.InternalServerError, detail);

    /// <inheritdoc cref="InternalServerError(string)"/>
    public static OperationDataResult<T> InternalServerError<T>(string detail = FailureMessages.InternalServerError)
        => new ErrorDataResult<T>(ResultTitles.InternalServerError, ResultStatus.InternalServerError, detail);

    /// <summary>Failure (501 Not Implemented).</summary>
    public static OperationResult NotImplemented(string detail = FailureMessages.NotImplemented)
        => new ErrorResult(ResultTitles.NotImplemented, ResultStatus.NotImplemented, detail);

    /// <inheritdoc cref="NotImplemented(string)"/>
    public static OperationDataResult<T> NotImplemented<T>(string detail = FailureMessages.NotImplemented)
        => new ErrorDataResult<T>(ResultTitles.NotImplemented, ResultStatus.NotImplemented, detail);

    /// <summary>Failure (502 Bad Gateway).</summary>
    public static OperationResult BadGateway(string detail = FailureMessages.BadGateway)
        => new ErrorResult(ResultTitles.BadGateway, ResultStatus.BadGateway, detail);

    /// <inheritdoc cref="BadGateway(string)"/>
    public static OperationDataResult<T> BadGateway<T>(string detail = FailureMessages.BadGateway)
        => new ErrorDataResult<T>(ResultTitles.BadGateway, ResultStatus.BadGateway, detail);

    /// <summary>Failure (503 Service Unavailable).</summary>
    public static OperationResult ServiceUnavailable(string detail = FailureMessages.ServiceUnavailable)
        => new ErrorResult(ResultTitles.ServiceUnavailable, ResultStatus.ServiceUnavailable, detail);

    /// <inheritdoc cref="ServiceUnavailable(string)"/>
    public static OperationDataResult<T> ServiceUnavailable<T>(string detail = FailureMessages.ServiceUnavailable)
        => new ErrorDataResult<T>(ResultTitles.ServiceUnavailable, ResultStatus.ServiceUnavailable, detail);

    /// <summary>Failure (504 Gateway Timeout).</summary>
    public static OperationResult GatewayTimeout(string detail = FailureMessages.GatewayTimeout)
        => new ErrorResult(ResultTitles.GatewayTimeout, ResultStatus.GatewayTimeout, detail);

    /// <inheritdoc cref="GatewayTimeout(string)"/>
    public static OperationDataResult<T> GatewayTimeout<T>(string detail = FailureMessages.GatewayTimeout)
        => new ErrorDataResult<T>(ResultTitles.GatewayTimeout, ResultStatus.GatewayTimeout, detail);

    /// <summary>Failure (505 HTTP Version Not Supported).</summary>
    public static OperationResult HttpVersionNotSupported(string detail = FailureMessages.HttpVersionNotSupported)
        => new ErrorResult(ResultTitles.HttpVersionNotSupported, ResultStatus.HttpVersionNotSupported, detail);

    /// <inheritdoc cref="HttpVersionNotSupported(string)"/>
    public static OperationDataResult<T> HttpVersionNotSupported<T>(string detail = FailureMessages.HttpVersionNotSupported)
        => new ErrorDataResult<T>(ResultTitles.HttpVersionNotSupported, ResultStatus.HttpVersionNotSupported, detail);

    /// <summary>Failure (506 Variant Also Negotiates).</summary>
    public static OperationResult VariantAlsoNegotiates(string detail = FailureMessages.VariantAlsoNegotiates)
        => new ErrorResult(ResultTitles.VariantAlsoNegotiates, ResultStatus.VariantAlsoNegotiates, detail);

    /// <inheritdoc cref="VariantAlsoNegotiates(string)"/>
    public static OperationDataResult<T> VariantAlsoNegotiates<T>(string detail = FailureMessages.VariantAlsoNegotiates)
        => new ErrorDataResult<T>(ResultTitles.VariantAlsoNegotiates, ResultStatus.VariantAlsoNegotiates, detail);

    /// <summary>Failure (507 Insufficient Storage).</summary>
    public static OperationResult InsufficientStorage(string detail = FailureMessages.InsufficientStorage)
        => new ErrorResult(ResultTitles.InsufficientStorage, ResultStatus.InsufficientStorage, detail);

    /// <inheritdoc cref="InsufficientStorage(string)"/>
    public static OperationDataResult<T> InsufficientStorage<T>(string detail = FailureMessages.InsufficientStorage)
        => new ErrorDataResult<T>(ResultTitles.InsufficientStorage, ResultStatus.InsufficientStorage, detail);

    /// <summary>Failure (508 Loop Detected).</summary>
    public static OperationResult LoopDetected(string detail = FailureMessages.LoopDetected)
        => new ErrorResult(ResultTitles.LoopDetected, ResultStatus.LoopDetected, detail);

    /// <inheritdoc cref="LoopDetected(string)"/>
    public static OperationDataResult<T> LoopDetected<T>(string detail = FailureMessages.LoopDetected)
        => new ErrorDataResult<T>(ResultTitles.LoopDetected, ResultStatus.LoopDetected, detail);

    /// <summary>Failure (510 Not Extended).</summary>
    public static OperationResult NotExtended(string detail = FailureMessages.NotExtended)
        => new ErrorResult(ResultTitles.NotExtended, ResultStatus.NotExtended, detail);

    /// <inheritdoc cref="NotExtended(string)"/>
    public static OperationDataResult<T> NotExtended<T>(string detail = FailureMessages.NotExtended)
        => new ErrorDataResult<T>(ResultTitles.NotExtended, ResultStatus.NotExtended, detail);

    /// <summary>Failure (511 Network Authentication Required).</summary>
    public static OperationResult NetworkAuthenticationRequired(string detail = FailureMessages.NetworkAuthenticationRequired)
        => new ErrorResult(ResultTitles.NetworkAuthenticationRequired, ResultStatus.NetworkAuthenticationRequired, detail);

    /// <inheritdoc cref="NetworkAuthenticationRequired(string)"/>
    public static OperationDataResult<T> NetworkAuthenticationRequired<T>(string detail = FailureMessages.NetworkAuthenticationRequired)
        => new ErrorDataResult<T>(ResultTitles.NetworkAuthenticationRequired, ResultStatus.NetworkAuthenticationRequired, detail);

    /// <summary>Escape hatch: a failure with a custom title, detail, and status, for anything not covered by a named factory.</summary>
    public static OperationResult Failure(string title, string detail, ResultStatus status)
        => new ErrorResult(title, status, detail);

    /// <inheritdoc cref="Failure(string, string, ResultStatus)"/>
    public static OperationDataResult<T> Failure<T>(string title, string detail, ResultStatus status)
        => new ErrorDataResult<T>(title, status, detail);

    /// <inheritdoc cref="Failure(string, string, ResultStatus)"/>
    public static OperationDataResult<T> Failure<T>(T data, string title, string detail, ResultStatus status)
        => new ErrorDataResult<T>(data, title, status, detail);
}
