using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ResultHandler.Mapping;

namespace ResultHandler.AspNetCore.Extensions;

/// <summary>Registers ASP.NET Core's Problem Details services so responses the framework produces on its own (401/403 from authentication, 404 for unknown routes, 405, 415, 429 from rate limiting, unhandled exceptions) have the same shape as failed results.</summary>
public static class ResultProblemDetailsServiceCollectionExtensions
{
    /// <summary>
    /// Calls <see cref="ProblemDetailsServiceCollectionExtensions.AddProblemDetails(IServiceCollection)"/> and adds a
    /// <see cref="ProblemDetailsOptions.CustomizeProblemDetails"/> step that fills a missing <c>type</c> with the same RFC URI a failed result
    /// uses, a missing <c>title</c> with the reason phrase, and adds <c>instance</c> and <c>traceId</c>. A <see cref="ProblemDetailsOptions.CustomizeProblemDetails"/>
    /// set in <paramref name="configure"/>, or anywhere else, runs afterwards. Safe to call more than once. Pair it with <c>app.UseStatusCodePages()</c>
    /// (bodyless 4xx/5xx) and <c>AddResultExceptionHandler()</c> plus <c>app.UseExceptionHandler()</c> (exceptions).
    /// </summary>
    /// <example>
    /// <code>
    /// builder.Services.AddResultProblemDetails();
    /// var app = builder.Build();
    /// app.UseStatusCodePages();
    /// </code>
    /// </example>
    public static IServiceCollection AddResultProblemDetails(this IServiceCollection services, Action<ProblemDetailsOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddProblemDetails();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<ProblemDetailsOptions>, ResultProblemDetailsPostConfigure>());
        return services;
    }

    /// <summary>Runs after every <c>Configure</c>, so the result-shaped defaults wrap whatever <see cref="ProblemDetailsOptions.CustomizeProblemDetails"/> the app set.</summary>
    private sealed class ResultProblemDetailsPostConfigure : IPostConfigureOptions<ProblemDetailsOptions>
    {
        public void PostConfigure(string? name, ProblemDetailsOptions options)
        {
            var customize = options.CustomizeProblemDetails;
            options.CustomizeProblemDetails = context =>
            {
                ApplyDefaults(context);
                customize?.Invoke(context);
            };
        }

        private static void ApplyDefaults(ProblemDetailsContext context)
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
        }
    }
}
