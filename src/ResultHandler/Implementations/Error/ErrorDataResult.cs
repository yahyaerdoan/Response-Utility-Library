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

    /// <summary><see cref="OperationResult.Errors"/> is populated with the flattened messages, for
    /// callers that only read the flat list.</summary>
    public ErrorDataResult(string title, ResultStatus status, IReadOnlyDictionary<string, IReadOnlyList<string>> fieldErrors)
        : base(default, false, status, title, null, [.. fieldErrors.SelectMany(pair => pair.Value)], fieldErrors)
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
}
