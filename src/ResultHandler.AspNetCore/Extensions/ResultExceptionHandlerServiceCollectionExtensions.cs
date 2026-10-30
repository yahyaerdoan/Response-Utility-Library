using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ResultHandler.AspNetCore.Exceptions;

namespace ResultHandler.AspNetCore.Extensions;

/// <summary>Registers an <see cref="IExceptionHandler"/> that answers unhandled exceptions with the same Problem Details as failed results.</summary>
public static class ResultExceptionHandlerServiceCollectionExtensions
{
    /// <summary>
    /// Registers the handler used by <c>app.UseExceptionHandler()</c> and <see cref="ResultProblemDetailsServiceCollectionExtensions.AddResultProblemDetails"/>.
    /// Mapped exceptions become the failed result their mapping returns (the closest mapped base type wins); <see cref="Microsoft.AspNetCore.Http.BadHttpRequestException"/>
    /// keeps its status code; everything else is a 500. Exception messages of unmapped exceptions reach the client only in Development
    /// (<see cref="ResultExceptionHandlerOptions.ExposeExceptionMessages"/>). Requests the client aborted are left to the middleware, which answers 499.
    /// Logging stays with the exception handler middleware: on .NET 10, which by default stops logging exceptions an <see cref="IExceptionHandler"/>
    /// handles, server errors (5xx) are logged again and client errors (4xx) are not, unless the app sets
    /// <see cref="ExceptionHandlerOptions"/>.<c>SuppressDiagnosticsCallback</c> itself. On .NET 8 and 9 the middleware logs every exception.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.Services.AddResultExceptionHandler(options =&gt; options
    ///     .Map&lt;DbUpdateConcurrencyException&gt;(_ =&gt; Result.Conflict("This record was changed by someone else.").WithCode("Concurrency"))
    ///     .Map&lt;KeyNotFoundException&gt;(exception =&gt; Result.NotFound(exception.Message)));
    /// var app = builder.Build();
    /// app.UseExceptionHandler();
    /// </code>
    /// </example>
    public static IServiceCollection AddResultExceptionHandler(this IServiceCollection services, Action<ResultExceptionHandlerOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddResultProblemDetails();
        services.AddOptions<ResultExceptionHandlerOptions>()
            .Configure<IHostEnvironment>((options, environment) => options.ExposeExceptionMessages = environment.IsDevelopment());
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IExceptionHandler, ResultExceptionHandler>());
#if NET10_0_OR_GREATER
        services.Configure<ExceptionHandlerOptions>(options =>
            options.SuppressDiagnosticsCallback ??= context => context.HttpContext.Response.StatusCode < StatusCodes.Status500InternalServerError);
#endif
        return services;
    }
}
