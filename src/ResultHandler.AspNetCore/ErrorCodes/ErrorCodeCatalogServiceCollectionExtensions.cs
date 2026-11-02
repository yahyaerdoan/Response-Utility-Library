using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ResultHandler.AspNetCore.Extensions;

namespace ResultHandler.AspNetCore.ErrorCodes;

/// <summary>Registers the error code catalog.</summary>
public static class ErrorCodeCatalogServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="ErrorCodeCatalog"/> as a singleton, built from the <see cref="ResultHandler.ErrorCodes.ErrorCodeAttribute"/> constants of
    /// the assemblies <paramref name="configure"/> adds. With <see cref="ErrorCodeCatalogOptions.LinkProblemTypes"/> on (the default), a failed
    /// response whose code is in the catalog gets its code's documentation URL as <c>"type"</c>. That step runs in <c>CustomizeProblemDetails</c>, so
    /// call this after anything that adds codes there. Serve the catalog with <c>MapErrorCodes()</c>.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.Services.AddResultProblemDetails();
    /// builder.Services.AddErrorCodeCatalog(options => options.AddAssemblyTree(typeof(Program).Assembly, "MyApp."));
    /// var app = builder.Build();
    /// app.MapErrorCodes();
    /// </code>
    /// </example>
    public static IServiceCollection AddErrorCodeCatalog(this IServiceCollection services, Action<ErrorCodeCatalogOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = services.AddOptions<ErrorCodeCatalogOptions>();
        if (configure is not null)
        {
            _ = options.Configure(configure);
        }

        services.TryAddSingleton(provider => new ErrorCodeCatalog(provider.GetRequiredService<IOptions<ErrorCodeCatalogOptions>>().Value.Assemblies));
        _ = services.AddProblemDetails().PostConfigure<ProblemDetailsOptions>(problemOptions =>
        {
            var customize = problemOptions.CustomizeProblemDetails;
            problemOptions.CustomizeProblemDetails = context =>
            {
                customize?.Invoke(context);
                LinkType(context);
            };
        });

        return services;
    }

    private static void LinkType(ProblemDetailsContext context)
    {
        var services = context.HttpContext.RequestServices;
        var options = services.GetRequiredService<IOptions<ErrorCodeCatalogOptions>>().Value;
        if (options.LinkProblemTypes
            && context.ProblemDetails.Extensions.TryGetValue(ResultProblemFields.Code, out var value)
            && value is string code
            && services.GetRequiredService<ErrorCodeCatalog>().Find(code) is not null)
        {
            context.ProblemDetails.Type = options.DocumentationUrl(context.HttpContext.Request, code);
        }
    }
}
