namespace ResultHandler.Core.Abstractions;

/// <summary>
/// Optional companion to <see cref="IOperationResult"/> for results that carry per-field validation
/// errors - kept as a separate interface (not a new member on <see cref="IOperationResult"/>) so
/// adding it doesn't break anyone implementing that interface directly. Check for it with a type
/// pattern (<c>result is IHasFieldErrors withFields</c>) since not every <see cref="IOperationResult"/>
/// implements it.
/// </summary>
public interface IHasFieldErrors
{
    /// <summary>Per-field validation errors, keyed by property name.</summary>
    IReadOnlyDictionary<string, IReadOnlyList<string>> FieldErrors { get; }
}
