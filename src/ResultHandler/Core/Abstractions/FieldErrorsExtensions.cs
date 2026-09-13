using System.Collections.Immutable;

namespace ResultHandler.Core.Abstractions;

/// <summary>Shared shortcut for the <c>result is IHasFieldErrors</c> check, so callers don't repeat it.</summary>
public static class FieldErrorsExtensions
{
    /// <summary><paramref name="result"/>'s <see cref="IHasFieldErrors.FieldErrors"/> if it has any, otherwise empty.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> GetFieldErrors(this IOperationResult result)
        => result is IHasFieldErrors { FieldErrors.Count: > 0 } withFieldErrors
            ? withFieldErrors.FieldErrors
            : ImmutableDictionary<string, IReadOnlyList<string>>.Empty;
}
