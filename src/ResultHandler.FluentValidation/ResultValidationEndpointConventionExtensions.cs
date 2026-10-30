using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ResultHandler.AspNetCore.Extensions;
using ResultHandler.Core.Base;

namespace ResultHandler.FluentValidation;

/// <summary>Validates Minimal API arguments with their registered FluentValidation validators before the handler runs.</summary>
public static class ResultValidationEndpointConventionExtensions
{
    /// <summary>
    /// Adds a filter to every endpoint in <paramref name="builder"/> (an endpoint or a whole route group) that validates each handler parameter
    /// whose type has a registered <see cref="global::FluentValidation.IValidator{T}"/>, and answers 422 validation Problem Details instead of
    /// calling the handler when any fails. Endpoints without validated parameters get no filter at all. Register validators as usual, for example
    /// with <c>AddValidatorsFromAssemblyContaining&lt;T&gt;()</c>.
    /// </summary>
    /// <example>
    /// <code>
    /// var api = app.MapGroup("/api").WithResultValidation().ProducesResultProblems();
    /// api.MapPost("/receipts", (CreateReceiptRequest request, ReceiptService receipts, CancellationToken ct) =&gt; receipts.CreateAsync(request, ct).ToResultAsync());
    /// </code>
    /// </example>
    public static TBuilder WithResultValidation<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddEndpointFilterFactory((context, next) =>
        {
            var validated = context.MethodInfo.GetParameters()
                .Select((parameter, index) => (parameter.ParameterType, Index: index))
                .Where(parameter => RequestValidation.HasValidator(context.ApplicationServices, parameter.ParameterType))
                .ToArray();

            if (validated.Length == 0)
            {
                return next;
            }

            return async invocation =>
            {
                var httpContext = invocation.HttpContext;
                var errors = await RequestValidation.ValidateAsync(
                    httpContext.RequestServices,
                    validated.Select(parameter => (parameter.ParameterType, invocation.Arguments[parameter.Index])),
                    httpContext.RequestAborted).ConfigureAwait(false);

                return errors is null
                    ? await next(invocation).ConfigureAwait(false)
                    : OperationResult.Failure(errors).ToProblemResult();
            };
        });

        return builder;
    }
}
