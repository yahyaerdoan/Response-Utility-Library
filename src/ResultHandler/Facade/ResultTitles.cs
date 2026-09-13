using ResultHandler.Core.Base;

namespace ResultHandler.Facade;

/// <summary>Fixed title/message text used by <see cref="Result"/>'s per-status factories.</summary>
internal static class ResultTitles
{
    // 1xx / 2xx / 3xx - Result.Success.cs
    public const string Continue = "Continue.";
    public const string SwitchingProtocols = "Switching protocols.";
    public const string Processing = "Processing.";
    public const string EarlyHints = "Early hints.";
    public const string Created = "Resource created successfully.";
    public const string Accepted = "Request accepted for processing.";
    public const string NoContent = "No content.";
    public const string ResetContent = "Reset content.";
    public const string NonAuthoritativeInformation = "Non-authoritative information.";
    public const string PartialContent = "Partial content.";
    public const string MultiStatus = "Multi-status.";
    public const string AlreadyReported = "Already reported.";
    public const string ImUsed = "IM used.";
    public const string NotModified = "Not modified.";

    // 4xx / 5xx - Result.Error.cs
    public const string BadRequest = "Bad Request";
    public const string Unauthorized = "Unauthorized";
    public const string PaymentRequired = "Payment Required";
    public const string Forbidden = "Forbidden";
    public const string NotFound = "Not Found";
    public const string MethodNotAllowed = "Method Not Allowed";
    public const string NotAcceptable = "Not Acceptable";
    public const string ProxyAuthenticationRequired = "Proxy Authentication Required";
    public const string RequestTimeout = "Request Timeout";
    public const string Conflict = "Conflict";
    public const string Gone = "Gone";
    public const string LengthRequired = "Length Required";
    public const string PreconditionFailed = "Precondition Failed";
    public const string ContentTooLarge = "Content Too Large";
    public const string UriTooLong = "URI Too Long";
    public const string UnsupportedMediaType = "Unsupported Media Type";
    public const string RangeNotSatisfiable = "Range Not Satisfiable";
    public const string ExpectationFailed = "Expectation Failed";
    public const string ImATeapot = "I'm a Teapot";
    public const string MisdirectedRequest = "Misdirected Request";
    public const string UnprocessableContent = "Unprocessable Content";
    public const string ValidationFailed = OperationResultDefaults.ValidationFailedTitle;
    public const string Locked = "Locked";
    public const string FailedDependency = "Failed Dependency";
    public const string TooEarly = "Too Early";
    public const string UpgradeRequired = "Upgrade Required";
    public const string PreconditionRequired = "Precondition Required";
    public const string TooManyRequests = "Too Many Requests";
    public const string RequestHeaderFieldsTooLarge = "Request Header Fields Too Large";
    public const string UnavailableForLegalReasons = "Unavailable For Legal Reasons";
    public const string InternalServerError = "Internal Server Error";
    public const string NotImplemented = "Not Implemented";
    public const string BadGateway = "Bad Gateway";
    public const string ServiceUnavailable = "Service Unavailable";
    public const string GatewayTimeout = "Gateway Timeout";
    public const string HttpVersionNotSupported = "HTTP Version Not Supported";
    public const string VariantAlsoNegotiates = "Variant Also Negotiates";
    public const string InsufficientStorage = "Insufficient Storage";
    public const string LoopDetected = "Loop Detected";
    public const string NotExtended = "Not Extended";
    public const string NetworkAuthenticationRequired = "Network Authentication Required";

    // 3xx redirect message templates - Result.Success.cs
    public const string MovedPermanentlyTemplate = "Resource moved permanently to: {0}";
    public const string FoundTemplate = "Resource found at: {0}";
    public const string SeeOtherTemplate = "See other resource at: {0}";
    public const string UseProxyTemplate = "Requested resource must be accessed through the proxy: {0}";
    public const string TemporaryRedirectTemplate = "Temporarily redirected to: {0}";
    public const string PermanentRedirectTemplate = "Permanently redirected to: {0}";
}
