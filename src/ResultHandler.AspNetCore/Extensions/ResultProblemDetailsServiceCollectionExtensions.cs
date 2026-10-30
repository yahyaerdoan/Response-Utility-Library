using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using ResultHandler.Mapping;

namespace ResultHandler.AspNetCore.Extensions;

/// <summary>Registers ASP.NET Core's Problem Details services so responses the framework produces on its own (401/403 from authentication, 404 for unknown routes, 405, 415, 429 from rate limiting, unhandled exceptions) have the same shape as failed results.</summary>
public static class ResultProblemDetailsServiceCollectionExtensions
{
    /// <summary>
    /// Calls <see cref="ProblemDetailsServiceCollectionExtensions.AddProblemDetails(IServiceCollection, Action{ProblemDetailsOptions}?)"/> with a
    /// <see cref="ProblemDetailsOptions.CustomizeProblemDetails"/> that fills a missing <c>type</c> with the same RFC URI a failed result uses,
    /// a missing <c>title</c> with the reason phrase, and adds <c>instance</c> and <c>traceId</c>. A <see cref="ProblemDetailsOptions.CustomizeProblemDetails"/>
    /// set in <paramref name="configure"/> runs afterwards. Pair it with <c>app.UseStatusCodePages()</c> (bodyless 4xx/5xx) and, if nothing else
    /// handles exceptions, <c>app.UseExceptionHandler()</c>.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.Services.AddResultProblemDetails();
    /// var app = builder.Build();
    /// app.UseExceptionHandler();
    /// app.UseStatusCodePages();
    /// </code>
    /// </example>
    public static IServiceCollection AddResultProblemDetails(this IServiceCollection services, Action<ProblemDetailsOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddProblemDetails(options =>
        {
            configure?.Invoke(options);
            var customize = options.CustomizeProblemDetails;
            options.CustomizeProblemDetails = context =>
            {
                var problem = context.ProblemDetails;
                if (problem.Status is int status)
                {
                    var resultStatus = status.ToResultStatus();
                    problem.Type ??= (int)resultStatus.ToHttpStatusCode() == status
                        ? AspNetCoreResultExtensions.TypeFor(resultStatus)
                        : "about:blank";

                    var reasonPhrase = ReasonPhrases.GetReasonPhrase(status);
                    if (problem.Title is null && reasonPhrase.Length > 0)
                    {
                        problem.Title = reasonPhrase;
                    }
                }

                AspNetCoreResultExtensions.Enrich(problem, context.HttpContext);
                customize?.Invoke(context);
            };
        });
    }
}
