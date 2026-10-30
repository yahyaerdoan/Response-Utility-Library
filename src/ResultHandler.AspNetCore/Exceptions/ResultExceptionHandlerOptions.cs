using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Base;
using ResultHandler.Facade;
using ResultHandler.Mapping;

namespace ResultHandler.AspNetCore.Exceptions;

/// <summary>How <c>AddResultExceptionHandler</c> turns an unhandled exception into a failed result, written as Problem Details.</summary>
public sealed class ResultExceptionHandlerOptions
{
    private const string _unreadableRequestMessage = "The request could not be read. Please check the request and try again.";

    private readonly Dictionary<Type, Func<Exception, HttpContext, IOperationResult>> _maps = [];

    /// <summary>Whether unmapped exceptions put <see cref="Exception.Message"/> in <c>detail</c>. Defaults to <see langword="true"/> only in the Development environment; elsewhere a generic message is used so internals never reach clients.</summary>
    public bool ExposeExceptionMessages { get; set; }

    /// <summary>Maps <typeparamref name="TException"/>, and any exception derived from it that has no closer mapping, to a failed result.</summary>
    /// <example>
    /// <code>
    /// options.Map&lt;OrderLockedException&gt;(exception =&gt; Result.Conflict(exception.Message).WithCode("Order.Locked"));
    /// </code>
    /// </example>
    public ResultExceptionHandlerOptions Map<TException>(Func<TException, IOperationResult> map)
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(map);
        return Map<TException>((exception, _) => map(exception));
    }

    /// <summary>Maps <typeparamref name="TException"/> to a failed result, with access to the request (for example to read the original endpoint).</summary>
    public ResultExceptionHandlerOptions Map<TException>(Func<TException, HttpContext, IOperationResult> map)
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(map);
        _maps[typeof(TException)] = (exception, httpContext) => map((TException)exception, httpContext);
        return this;
    }

    /// <summary>
    /// The failed result <paramref name="exception"/> is answered with: the closest mapping by type (the exception's own type first, then each base
    /// type), else the built-in fallback. <see cref="BadHttpRequestException"/> keeps its status code and names the offending field when it can;
    /// <see cref="HttpRequestException"/> is a 502 and a timeout (<see cref="TimeoutException"/>, or an <c>HttpClient</c> timeout) a 504, since the
    /// failure is in a service this one depends on; everything else is a 500. Mappings should be free of side effects: they may run more than once
    /// per exception, for example when a logging handler asks for the status first.
    /// </summary>
    public IOperationResult Resolve(Exception exception, HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(httpContext);

        for (var type = exception.GetType(); type is not null && type != typeof(object); type = type.BaseType)
        {
            if (_maps.TryGetValue(type, out var map))
            {
                var mapped = map(exception, httpContext);
                return mapped.IsSuccessful
                    ? throw new InvalidOperationException($"The exception mapping for {type.Name} returned a successful result; it must return a failure.")
                    : mapped;
            }
        }

        return exception switch
        {
            BadHttpRequestException badRequest => FromBadRequest(badRequest),
            HttpRequestException => ExposeExceptionMessages ? Result.BadGateway(exception.Message) : Result.BadGateway(),
            TimeoutException or TaskCanceledException { InnerException: TimeoutException } => ExposeExceptionMessages ? Result.GatewayTimeout(exception.Message) : Result.GatewayTimeout(),
            _ => ExposeExceptionMessages ? Result.InternalServerError(exception.Message) : Result.InternalServerError(),
        };
    }

    private OperationResult FromBadRequest(BadHttpRequestException exception)
    {
        var status = exception.StatusCode.ToResultStatus();
        var reasonPhrase = ReasonPhrases.GetReasonPhrase(exception.StatusCode);
        var title = reasonPhrase.Length > 0 ? reasonPhrase : status.ToString();
        var detail = ExposeExceptionMessages ? exception.Message : _unreadableRequestMessage;
        return RequestBindingErrors.From(exception) is { } fieldErrors
            ? new OperationResult(false, status, title, detail, fieldErrors)
            : OperationResult.Failure(title, detail, status);
    }
}
