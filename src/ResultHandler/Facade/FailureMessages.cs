namespace ResultHandler.Facade;

/// <summary>Default <c>detail</c> text for optional-parameter failure factories in <see cref="Result"/> and <see cref="ResultHandler.Functional.ResultFailureFactory"/>.</summary>
internal static class FailureMessages
{
    public const string Unauthorized = "Authentication is required to access this resource.";
    public const string Forbidden = "You do not have permission to access this resource.";
    public const string MethodNotAllowed = "The HTTP method used is not allowed for this endpoint.";
    public const string NotAcceptable = "The requested media type is not supported by this server.";
    public const string ProxyAuthenticationRequired = "Proxy authentication is required to access this resource.";
    public const string RequestTimeout = "The request timed out. Please try again.";
    public const string LengthRequired = "A Content-Length header is required for this request.";
    public const string UriTooLong = "The request URI exceeds the maximum allowed length.";
    public const string UnsupportedMediaType = "The submitted media type is not supported. Please use a supported type.";
    public const string ExpectationFailed = "The expectation given in the Expect header could not be met.";
    public const string ImATeapot = "I'm a teapot.";
    public const string MisdirectedRequest = "The request was directed to a server unable to produce a response.";
    public const string Locked = "The resource is currently locked and cannot be modified.";
    public const string TooEarly = "The request was rejected because it may be replayed.";
    public const string TooManyRequests = "Too many requests. Please slow down and try again later.";
    public const string RequestHeaderFieldsTooLarge = "One or more request header fields exceed the maximum allowed size.";
    public const string InternalServerError = "An unexpected error occurred. Please try again later.";
    public const string NotImplemented = "This feature is not yet implemented.";
    public const string BadGateway = "An upstream server returned an invalid response.";
    public const string ServiceUnavailable = "The service is temporarily unavailable. Please try again later.";
    public const string GatewayTimeout = "An upstream server did not respond in time.";
    public const string HttpVersionNotSupported = "The HTTP version used in the request is not supported.";
    public const string VariantAlsoNegotiates = "The server has an internal configuration error.";
    public const string InsufficientStorage = "Insufficient storage to complete the request.";
    public const string LoopDetected = "The server detected an infinite loop while processing the request.";
    public const string NotExtended = "Further extensions to the request are required.";
    public const string NetworkAuthenticationRequired = "Network authentication is required to access this resource.";
}
