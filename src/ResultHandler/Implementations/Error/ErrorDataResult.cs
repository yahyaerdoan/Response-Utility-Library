using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;

namespace ResultHandler.Implementations.Error;

/// <summary>A failed <see cref="OperationDataResult{T}"/> (<c>IsSuccessful</c> is always <see langword="false"/>).</summary>
public class ErrorDataResult<T> : OperationDataResult<T>
{
    /// <summary>Default error, no data: status <see cref="ResultStatus.InternalServerError"/>, title "An error occurred.".</summary>
    public ErrorDataResult()
        : base(default, false, ResultStatus.InternalServerError, OperationResultDefaults.ErrorTitle)
    {
    }

    /// <summary>Default error, carrying <paramref name="data"/>: status <see cref="ResultStatus.InternalServerError"/>, title "An error occurred.".</summary>
    public ErrorDataResult(T data)
        : base(data, false, ResultStatus.InternalServerError, OperationResultDefaults.ErrorTitle)
    {
    }

    /// <summary>Error with a title and status, no data, detail, or errors.</summary>
    public ErrorDataResult(string title, ResultStatus status)
        : base(default, false, status, title)
    {
    }

    /// <summary>Error with a title, status, and additional detail text, no data.</summary>
    public ErrorDataResult(string title, ResultStatus status, string detail)
        : base(default, false, status, title, detail)
    {
    }

    /// <summary>Error with a title, status, and a flat list of individual error messages, no data.</summary>
    public ErrorDataResult(string title, ResultStatus status, IReadOnlyList<string> errors)
        : base(default, false, status, title, null, errors)
    {
    }

    /// <summary>Also flattens <paramref name="fieldErrors"/> into <see cref="OperationResult.Errors"/>, for callers that only read the flat list.</summary>
    public ErrorDataResult(string title, ResultStatus status, IReadOnlyDictionary<string, IReadOnlyList<string>> fieldErrors)
        : base(default, false, status, title, null, OperationResultDefaults.FlattenFieldErrors(fieldErrors), fieldErrors)
    {
    }

    /// <summary>Error with a title and status, carrying <paramref name="data"/>, no detail.</summary>
    public ErrorDataResult(T data, string title, ResultStatus status)
        : base(data, false, status, title)
    {
    }

    /// <summary>Error with a title, status, and additional detail text, carrying <paramref name="data"/>.</summary>
    public ErrorDataResult(T data, string title, ResultStatus status, string detail)
        : base(data, false, status, title, detail)
    {
    }

    /// <summary>Carries <paramref name="errors"/> and <paramref name="fieldErrors"/> independently, for re-projecting a failure that already has its own flat message list. Internal — used only by <c>ResultExtensions.Propagate</c>.</summary>
    internal ErrorDataResult(string title, ResultStatus status, IReadOnlyList<string> errors, IReadOnlyDictionary<string, IReadOnlyList<string>> fieldErrors)
        : base(default, false, status, title, null, errors, fieldErrors)
    {
    }
}
