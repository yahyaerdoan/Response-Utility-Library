namespace ResultHandler.Core.Abstractions;

/// <summary>Optional companion to <see cref="IOperationResult"/> for results carrying per-field validation errors. Not every <see cref="IOperationResult"/> implements it — check with <c>result is IHasFieldErrors withFields</c>.</summary>
public interface IHasFieldErrors
{
    /// <summary>Per-field validation errors, keyed by property name.</summary>
    IReadOnlyDictionary<string, IReadOnlyList<string>> FieldErrors { get; }
}
