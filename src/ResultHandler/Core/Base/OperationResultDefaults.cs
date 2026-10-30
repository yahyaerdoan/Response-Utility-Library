using System.Collections.Immutable;

namespace ResultHandler.Core.Base;

/// <summary>Default title text and error-map helpers shared by <see cref="OperationResult"/>, <see cref="OperationDataResult{T}"/> and their success/error subclasses.</summary>
internal static class OperationResultDefaults
{
    public const string SuccessTitle = "Operation completed successfully.";
    public const string ErrorTitle = "An error occurred.";
    public const string ValidationFailedTitle = "Validation Failed";

    /// <summary>The key of messages that belong to no field.</summary>
    public const string GeneralErrorKey = "";

    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> NoErrors = ImmutableDictionary<string, IReadOnlyList<string>>.Empty;

    /// <summary><paramref name="errors"/> without fields that carry no messages; <see cref="NoErrors"/> for <see langword="null"/> or empty.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> Normalize(IReadOnlyDictionary<string, IReadOnlyList<string>>? errors)
    {
        if (errors is null || errors.Count == 0)
        {
            return NoErrors;
        }

        return errors.Values.All(messages => messages is { Count: > 0 })
            ? errors
            : errors.Where(pair => pair.Value is { Count: > 0 }).ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
    }

    /// <summary>Messages that belong to no field, as an error map under <see cref="GeneralErrorKey"/>.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> General(IReadOnlyList<string> messages)
        => messages is { Count: > 0 }
            ? new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal) { [GeneralErrorKey] = messages }
            : NoErrors;
}
