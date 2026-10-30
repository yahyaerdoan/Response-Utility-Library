using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace ResultHandler.AspNetCore.Extensions;

/// <summary>MVC result for a failed operation: adds the request's instance and trace id, then writes through <see cref="IProblemDetailsService"/> when one is registered, falling back to normal <see cref="ObjectResult"/> content negotiation.</summary>
internal sealed class ProblemDetailsObjectResult : ObjectResult
{
    public ProblemDetailsObjectResult(ProblemDetails problemDetails)
        : base(problemDetails)
    {
        ProblemDetails = problemDetails;
        StatusCode = problemDetails.Status;
    }

    public ProblemDetails ProblemDetails { get; }

    public override async Task ExecuteResultAsync(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var httpContext = context.HttpContext;
        AspNetCoreResultExtensions.Enrich(ProblemDetails, httpContext);

        var problemDetailsService = httpContext.RequestServices?.GetService<IProblemDetailsService>();
        if (problemDetailsService is not null)
        {
            httpContext.Response.StatusCode = StatusCode ?? StatusCodes.Status500InternalServerError;
            var written = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = ProblemDetails }).ConfigureAwait(false);
            if (written)
            {
                return;
            }
        }

        await base.ExecuteResultAsync(context).ConfigureAwait(false);
    }
}
