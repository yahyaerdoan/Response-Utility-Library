using Microsoft.AspNetCore.Diagnostics;
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
        return services;
    }
}
