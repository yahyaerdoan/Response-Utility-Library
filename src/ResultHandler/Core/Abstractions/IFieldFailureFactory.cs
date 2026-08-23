namespace ResultHandler.Core.Abstractions;

/// <summary>
/// Companion to <see cref="IResultFailureFactory{TSelf}"/> for the per-field validation error shape -
/// kept as a separate interface (not a new member on <see cref="IResultFailureFactory{TSelf}"/>) so
/// adding it doesn't break anyone implementing that interface directly.
/// </summary>
/// <typeparam name="TSelf">The implementing result type itself (CRTP).</typeparam>
public interface IFieldFailureFactory<TSelf>
    where TSelf : IOperationResult
{
    /// <summary>Builds a failed <typeparamref name="TSelf"/> for one or more per-field validation errors.</summary>
    /// <param name="fieldErrors">The error messages, keyed by the invalid field's property name.</param>
    /// <returns>A failed result whose <see cref="IHasFieldErrors.FieldErrors"/> is set to
    /// <paramref name="fieldErrors"/> and whose <see cref="IOperationResult.Errors"/> is set to the
    /// flattened messages, for callers that only read the flat list.</returns>
    /// <example>
    /// Short-circuiting a MediatR pipeline behavior once per-field validation fails:
    /// <code>
    /// where TResponse : IOperationResult, IFieldFailureFactory&lt;TResponse&gt;
    /// ...
    /// if (fieldErrors.Count > 0)
    /// {
    ///     return TResponse.Failure(fieldErrors);
    /// }
    /// </code>
    /// </example>
    static abstract TSelf Failure(IReadOnlyDictionary<string, IReadOnlyList<string>> fieldErrors);
}
