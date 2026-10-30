using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ResultHandler.AspNetCore.Extensions;

namespace ResultHandler.AspNetCore.Exceptions;

/// <summary>Writes every exception that reaches <c>UseExceptionHandler()</c> as the Problem Details of the failed result <see cref="ResultExceptionHandlerOptions"/> maps it to. Logging stays with the exception handler middleware.</summary>
internal sealed class ResultExceptionHandler(IOptions<ResultExceptionHandlerOptions> options) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var result = options.Value.Resolve(exception, httpContext);
        await result.ToProblemResult().ExecuteAsync(httpContext).ConfigureAwait(false);
        return true;
    }
}
