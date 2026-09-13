using ResultHandler.Core.Abstractions;

namespace ResultHandler.Core.Base;

/// <summary>Default title text shared by <see cref="OperationResult"/>, <see cref="OperationDataResult{T}"/> and their success/error subclasses.</summary>
internal static class OperationResultDefaults
{
    public const string SuccessTitle = "Operation completed successfully.";
    public const string ErrorTitle = "An error occurred.";
    public const string ValidationFailedTitle = "Validation Failed";

    /// <summary>Flattens per-field errors into one ordered list, for callers that only read <see cref="IOperationResult.Errors"/>.</summary>
    public static IReadOnlyList<string> FlattenFieldErrors(IReadOnlyDictionary<string, IReadOnlyList<string>> fieldErrors)
        => [.. fieldErrors.Values.Where(value => value is not null).SelectMany(value => value)];
}
