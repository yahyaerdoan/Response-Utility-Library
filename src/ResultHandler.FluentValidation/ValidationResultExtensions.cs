using FluentValidation.Results;
using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Base;
using ResultHandler.Facade;

namespace ResultHandler.FluentValidation;

/// <summary>Turns a FluentValidation <see cref="ValidationResult"/> into the field-keyed failures ResultHandler writes as validation Problem Details.</summary>
public static class ValidationResultExtensions
{
    /// <summary>The failures of <paramref name="validationResult"/> keyed by property name (<c>"Address.City"</c>, <c>"Items[0].Name"</c>; <c>""</c> for rules without one), each with its distinct messages in order.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> ToFieldErrors(this ValidationResult validationResult)
    {
        ArgumentNullException.ThrowIfNull(validationResult);

        return validationResult.Errors
            .GroupBy(failure => failure.PropertyName ?? string.Empty, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, IReadOnlyList<string> (group) => [.. group.Select(failure => failure.ErrorMessage).Distinct(StringComparer.Ordinal)], StringComparer.Ordinal);
    }

    /// <summary>A success when <paramref name="validationResult"/> is valid, otherwise a 422 failure carrying its field errors.</summary>
    /// <example>
    /// <code>
    /// var validation = await validator.ValidateAsync(request, cancellationToken);
    /// if (!validation.IsValid)
    /// {
    ///     return validation.ToOperationResult();
    /// }
    /// </code>
    /// </example>
    public static OperationResult ToOperationResult(this ValidationResult validationResult)
    {
        ArgumentNullException.ThrowIfNull(validationResult);
        return validationResult.IsValid ? Result.Success() : OperationResult.Failure(validationResult.ToFieldErrors());
    }

    /// <summary>The 422 failure of an invalid <paramref name="validationResult"/> as <typeparamref name="TResult"/>, for code that returns a data result (<c>validation.ToFailure&lt;OperationDataResult&lt;OrderResponse&gt;&gt;()</c>) or only knows the type parameter.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="validationResult"/> is valid, so there is no failure to return.</exception>
    public static TResult ToFailure<TResult>(this ValidationResult validationResult)
        where TResult : IOperationResult, IResultFailureFactory<TResult>
    {
        ArgumentNullException.ThrowIfNull(validationResult);
        return validationResult.IsValid
            ? throw new InvalidOperationException("The validation result is valid; there is no failure to return.")
            : TResult.Failure(validationResult.ToFieldErrors());
    }
}
