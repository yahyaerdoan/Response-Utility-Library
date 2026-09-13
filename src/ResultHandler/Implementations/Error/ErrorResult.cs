using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;

namespace ResultHandler.Implementations.Error;

/// <summary>A failed <see cref="OperationResult"/> (<c>IsSuccessful</c> is always <see langword="false"/>).</summary>
public class ErrorResult : OperationResult
{
    /// <summary>Default error: status <see cref="ResultStatus.InternalServerError"/>, title "An error occurred.".</summary>
    public ErrorResult()
        : base(false, ResultStatus.InternalServerError, OperationResultDefaults.ErrorTitle)
    {
    }

    /// <summary>Error with a title and status, no detail or errors.</summary>
    public ErrorResult(string title, ResultStatus status)
        : base(false, status, title)
    {
    }

    /// <summary>Error with a title, status, and additional detail text.</summary>
    public ErrorResult(string title, ResultStatus status, string detail)
        : base(false, status, title, detail)
    {
    }

    /// <summary>Error with a title, status, and a flat list of individual error messages.</summary>
    public ErrorResult(string title, ResultStatus status, IReadOnlyList<string> errors)
        : base(false, status, title, null, errors)
    {
    }

    /// <summary><see cref="OperationResult.Errors"/> is populated with the flattened messages, for
    /// callers that only read the flat list.</summary>
    public ErrorResult(string title, ResultStatus status, IReadOnlyDictionary<string, IReadOnlyList<string>> fieldErrors)
        : base(false, status, title, null, [.. fieldErrors.SelectMany(pair => pair.Value)], fieldErrors)
    {
    }
}
