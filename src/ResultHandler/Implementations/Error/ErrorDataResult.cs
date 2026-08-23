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

    public ErrorDataResult(T data)
        : base(data, false, ResultStatus.InternalServerError, OperationResultDefaults.ErrorTitle)
    {
    }

    public ErrorDataResult(string title, ResultStatus status)
        : base(default, false, status, title)
    {
    }

    public ErrorDataResult(string title, ResultStatus status, string detail)
        : base(default, false, status, title, detail)
    {
    }

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

    public ErrorDataResult(T data, string title, ResultStatus status)
        : base(data, false, status, title)
    {
    }

    public ErrorDataResult(T data, string title, ResultStatus status, string detail)
        : base(data, false, status, title, detail)
    {
    }
}
