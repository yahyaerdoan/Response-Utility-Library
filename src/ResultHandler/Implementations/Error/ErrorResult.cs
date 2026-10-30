using System.Text.Json.Serialization;
using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;
using ResultHandler.Serialization;

namespace ResultHandler.Implementations.Error;

/// <summary>A failed <see cref="OperationResult"/> (<c>IsSuccessful</c> is always <see langword="false"/>).</summary>
[JsonConverter(typeof(OperationResultJsonConverterFactory))]
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

    /// <summary>Error with a title, status, and messages that belong to no field.</summary>
    public ErrorResult(string title, ResultStatus status, IReadOnlyList<string> errors)
        : base(false, status, title, null, OperationResultDefaults.General(errors))
    {
    }

    /// <summary>Error with a title, status, and error messages by field (<c>""</c> for messages that belong to no field).</summary>
    public ErrorResult(string title, ResultStatus status, IReadOnlyDictionary<string, IReadOnlyList<string>> errors)
        : base(false, status, title, null, errors)
    {
    }

    /// <summary>Error carrying every part of another failure, for re-projecting it into this type. Internal: used only by <c>ResultExtensions.Propagate</c>.</summary>
    internal ErrorResult(string title, ResultStatus status, string? detail, IReadOnlyDictionary<string, IReadOnlyList<string>> errors)
        : base(false, status, title, detail, errors)
    {
    }
}
