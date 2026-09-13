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
    /// <summary>Builds a failed <typeparamref name="TSelf"/> from per-field validation errors, keyed by property name. Also populates <see cref="IOperationResult.Errors"/> with the flattened messages.</summary>
    static abstract TSelf Failure(IReadOnlyDictionary<string, IReadOnlyList<string>> fieldErrors);
}
