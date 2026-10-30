using System.Text.Json.Serialization;
using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;
using ResultHandler.Serialization;

namespace ResultHandler.Implementations.Error;

/// <summary>A failed <see cref="OperationDataResult{T}"/> (<c>IsSuccessful</c> is always <see langword="false"/>).</summary>
[JsonConverter(typeof(OperationResultJsonConverterFactory))]
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

    /// <summary>Error with a title, status, and messages that belong to no field, no data.</summary>
    public ErrorDataResult(string title, ResultStatus status, IReadOnlyList<string> errors)
        : base(default, false, status, title, null, OperationResultDefaults.General(errors))
    {
    }

    /// <summary>Error with a title, status, and error messages by field (<c>""</c> for messages that belong to no field).</summary>
    public ErrorDataResult(string title, ResultStatus status, IReadOnlyDictionary<string, IReadOnlyList<string>> errors)
        : base(default, false, status, title, null, errors)
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

    /// <summary>Error carrying every part of another failure, for re-projecting it into this type. Internal: used only by <c>ResultExtensions.Propagate</c>.</summary>
    internal ErrorDataResult(string title, ResultStatus status, string? detail, IReadOnlyDictionary<string, IReadOnlyList<string>> errors)
        : base(default, false, status, title, detail, errors)
    {
    }
}
