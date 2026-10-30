using Microsoft.Extensions.DependencyInjection;

namespace ResultHandler.FluentValidation;

/// <summary>Validates MVC action arguments with their registered FluentValidation validators.</summary>
public static class ResultValidationMvcBuilderExtensions
{
    /// <summary>Adds a global action filter that validates each action argument whose type has a registered <see cref="global::FluentValidation.IValidator{T}"/> and returns 422 validation Problem Details, the same body as a failed result, when any fails. It runs after model binding, so <c>[ApiController]</c>'s own 400 for unreadable input still comes first.</summary>
    /// <example>
    /// <code>
    /// builder.Services.AddValidatorsFromAssemblyContaining&lt;CreateReceiptRequestValidator&gt;();
    /// builder.Services.AddControllers().AddResultValidation();
    /// </code>
    /// </example>
    public static IMvcBuilder AddResultValidation(this IMvcBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddMvcOptions(options => options.Filters.Add<ResultValidationActionFilter>());
    }
}
