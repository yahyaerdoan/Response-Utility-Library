using ResultHandler.Core.Enums;
using ResultHandler.ErrorCodes;

namespace ResultHandler.AspNetCore.Exceptions;

/// <summary>The codes <c>AddResultExceptionHandler</c> gives its built-in answers when <see cref="ResultExceptionHandlerOptions.IncludeErrorCodes"/> is on, so clients can branch on them, and the code of an unknown code asked of the error code catalog.</summary>
public static class ResultErrorCodes
{
    /// <summary>The request could not be read: a value in the wrong format, a missing required value, malformed JSON or a missing body.</summary>
    [ErrorCode(ResultStatus.BadRequest, "The request couldn't be read, for example malformed JSON or a parameter of the wrong type.", General = true)]
    public const string InvalidRequest = "Request.Invalid";

    /// <summary>A service this one depends on failed (502).</summary>
    [ErrorCode(ResultStatus.BadGateway, "A service the API depends on failed; try again later.", General = true)]
    public const string UpstreamFailed = "Upstream.Failed";

    /// <summary>A service this one depends on did not answer in time (504).</summary>
    [ErrorCode(ResultStatus.GatewayTimeout, "A service the API depends on didn't answer in time; try again later.", General = true)]
    public const string UpstreamTimeout = "Upstream.Timeout";

    /// <summary>An unexpected error (500).</summary>
    [ErrorCode(ResultStatus.InternalServerError, "Something unexpected failed on the server; report the traceId if it keeps happening.", General = true)]
    public const string ServerError = "Server.Error";

    /// <summary>The error code catalog has no such code (404).</summary>
    [ErrorCode(ResultStatus.NotFound, "The error code doesn't exist; the error code list has every code.")]
    public const string UnknownErrorCode = "ErrorCode.Unknown";
}
