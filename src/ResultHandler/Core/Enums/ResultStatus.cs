namespace ResultHandler.Core.Enums;

/// <summary>Every standard 1xx-5xx HTTP status, decoupled from <see cref="System.Net.HttpStatusCode"/> so the library has no hard ASP.NET Core dependency.</summary>
public enum ResultStatus
{
    // 1xx - Informational

    /// <summary>HTTP 100.</summary>
    Continue,

    /// <summary>HTTP 101.</summary>
    SwitchingProtocols,

    /// <summary>HTTP 102.</summary>
    Processing,

    /// <summary>HTTP 103.</summary>
    EarlyHints,

    // 2xx - Success

    /// <summary>HTTP 200.</summary>
    Ok,

    /// <summary>HTTP 201.</summary>
    Created,

    /// <summary>HTTP 202.</summary>
    Accepted,

    /// <summary>HTTP 203.</summary>
    NonAuthoritativeInformation,

    /// <summary>HTTP 204.</summary>
    NoContent,

    /// <summary>HTTP 205.</summary>
    ResetContent,

    /// <summary>HTTP 206.</summary>
    PartialContent,

    /// <summary>HTTP 207.</summary>
    MultiStatus,

    /// <summary>HTTP 208.</summary>
    AlreadyReported,

    /// <summary>HTTP 226.</summary>
    ImUsed,

    // 3xx - Redirection

    /// <summary>HTTP 300.</summary>
    MultipleChoices,

    /// <summary>HTTP 301.</summary>
    MovedPermanently,

    /// <summary>HTTP 302.</summary>
    Found,

    /// <summary>HTTP 303.</summary>
    SeeOther,

    /// <summary>HTTP 304.</summary>
    NotModified,

    /// <summary>HTTP 305.</summary>
    UseProxy,

    /// <summary>HTTP 307.</summary>
    TemporaryRedirect,

    /// <summary>HTTP 308.</summary>
    PermanentRedirect,

    // 4xx - Client errors

    /// <summary>HTTP 400.</summary>
    BadRequest,

    /// <summary>HTTP 401.</summary>
    Unauthorized,

    /// <summary>HTTP 402.</summary>
    PaymentRequired,

    /// <summary>HTTP 403.</summary>
    Forbidden,

    /// <summary>HTTP 404.</summary>
    NotFound,

    /// <summary>HTTP 405.</summary>
    MethodNotAllowed,

    /// <summary>HTTP 406.</summary>
    NotAcceptable,

    /// <summary>HTTP 407.</summary>
    ProxyAuthenticationRequired,

    /// <summary>HTTP 408.</summary>
    RequestTimeout,

    /// <summary>HTTP 409.</summary>
    Conflict,

    /// <summary>HTTP 410.</summary>
    Gone,

    /// <summary>HTTP 411.</summary>
    LengthRequired,

    /// <summary>HTTP 412.</summary>
    PreconditionFailed,

    /// <summary>HTTP 413.</summary>
    ContentTooLarge,

    /// <summary>HTTP 414.</summary>
    UriTooLong,

    /// <summary>HTTP 415.</summary>
    UnsupportedMediaType,

    /// <summary>HTTP 416.</summary>
    RangeNotSatisfiable,

    /// <summary>HTTP 417.</summary>
    ExpectationFailed,

    /// <summary>HTTP 418.</summary>
    ImATeapot,

    /// <summary>HTTP 421.</summary>
    MisdirectedRequest,

    /// <summary>HTTP 422.</summary>
    UnprocessableContent,

    /// <summary>HTTP 423.</summary>
    Locked,

    /// <summary>HTTP 424.</summary>
    FailedDependency,

    /// <summary>HTTP 425.</summary>
    TooEarly,

    /// <summary>HTTP 426.</summary>
    UpgradeRequired,

    /// <summary>HTTP 428.</summary>
    PreconditionRequired,

    /// <summary>HTTP 429.</summary>
    TooManyRequests,

    /// <summary>HTTP 431.</summary>
    RequestHeaderFieldsTooLarge,

    /// <summary>HTTP 451.</summary>
    UnavailableForLegalReasons,

    // 5xx - Server errors

    /// <summary>HTTP 500.</summary>
    InternalServerError,

    /// <summary>HTTP 501.</summary>
    NotImplemented,

    /// <summary>HTTP 502.</summary>
    BadGateway,

    /// <summary>HTTP 503.</summary>
    ServiceUnavailable,

    /// <summary>HTTP 504.</summary>
    GatewayTimeout,

    /// <summary>HTTP 505.</summary>
    HttpVersionNotSupported,

    /// <summary>HTTP 506.</summary>
    VariantAlsoNegotiates,

    /// <summary>HTTP 507.</summary>
    InsufficientStorage,

    /// <summary>HTTP 508.</summary>
    LoopDetected,

    /// <summary>HTTP 510.</summary>
    NotExtended,

    /// <summary>HTTP 511.</summary>
    NetworkAuthenticationRequired,
}
