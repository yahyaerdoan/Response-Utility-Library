using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ResultHandler.AspNetCore.Extensions;

/// <summary>
/// MVC result for a failed operation: adds the request's instance and trace id, applies <see cref="ProblemDetailsOptions.CustomizeProblemDetails"/>,
/// then writes through normal <see cref="ObjectResult"/> content negotiation, the same path MVC's own <c>ValidationProblem()</c> takes. It does not
/// go through <see cref="IProblemDetailsService"/>: MVC's writer rebuilds the body as a plain <see cref="ProblemDetails"/> and drops
/// <see cref="HttpValidationProblemDetails.Errors"/>.
/// </summary>
internal sealed class ProblemDetailsObjectResult : ObjectResult
{
    public ProblemDetailsObjectResult(ProblemDetails problemDetails)
        : base(problemDetails)
    {
        ProblemDetails = problemDetails;
        StatusCode = problemDetails.Status;
    }

    public ProblemDetails ProblemDetails { get; }

    public override Task ExecuteResultAsync(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var httpContext = context.HttpContext;
        AspNetCoreResultExtensions.Enrich(ProblemDetails, httpContext);

        var customize = httpContext.RequestServices?.GetService<IOptions<ProblemDetailsOptions>>()?.Value.CustomizeProblemDetails;
        customize?.Invoke(new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = ProblemDetails });

        return base.ExecuteResultAsync(context);
    }
}
