using Microsoft.AspNetCore.OpenApi;

namespace ResultHandler.AspNetCore.OpenApi;

/// <summary>Documents ResultHandler's responses in a Microsoft.AspNetCore.OpenApi document.</summary>
public static class ResultHandlerOpenApiOptionsExtensions
{
    /// <summary>
    /// Describes the success envelopes and Problem Details as they are written (always-written members required, nothing null, every member
    /// described), documents <see cref="ResultHandler.Core.Enums.ResultStatus"/> as an integer, and gives every documented failure a filled-in
    /// example. With <c>AddErrorCodeCatalog()</c> registered, each example carries a code from the catalog and links its <c>"type"</c> to the code's
    /// documentation.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.Services.AddOpenApi(options => options.AddResultHandlerDocumentation(settings => settings.EveryProblemHasCode = true));
    /// </code>
    /// </example>
    public static OpenApiOptions AddResultHandlerDocumentation(this OpenApiOptions options, Action<ResultHandlerOpenApiOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        var settings = new ResultHandlerOpenApiOptions();
        configure?.Invoke(settings);

        var bodies = new ResultBodySchemaTransformer(settings);
        return options
            .AddSchemaTransformer(bodies)
            .AddDocumentTransformer(bodies)
            .AddOperationTransformer(new ProblemExampleTransformer());
    }
}
