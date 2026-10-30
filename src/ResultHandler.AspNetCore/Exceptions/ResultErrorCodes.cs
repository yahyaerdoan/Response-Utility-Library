namespace ResultHandler.AspNetCore.Exceptions;

/// <summary>The codes <c>AddResultExceptionHandler</c> gives its built-in answers when <see cref="ResultExceptionHandlerOptions.IncludeErrorCodes"/> is on, so clients can branch on them.</summary>
public static class ResultErrorCodes
{
    /// <summary>The request could not be read: a value in the wrong format, a missing required value, malformed JSON or a missing body.</summary>
    public const string InvalidRequest = "Request.Invalid";

    /// <summary>A service this one depends on failed (502).</summary>
    public const string UpstreamFailed = "Upstream.Failed";

    /// <summary>A service this one depends on did not answer in time (504).</summary>
    public const string UpstreamTimeout = "Upstream.Timeout";

    /// <summary>An unexpected error (500).</summary>
    public const string ServerError = "Server.Error";
}
