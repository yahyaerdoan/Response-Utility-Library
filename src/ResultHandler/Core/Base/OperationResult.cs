using System.Collections.Immutable;
using System.Text.Json.Serialization;
using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Enums;
using ResultHandler.Implementations.Error;
using ResultHandler.Mapping;
using ResultHandler.Serialization;

namespace ResultHandler.Core.Base;

/// <summary>
/// Base implementation of <see cref="IOperationResult"/>. Immutable; prefer the
/// <see cref="Implementations.Success.SuccessResult"/>/<see cref="Implementations.Error.ErrorResult"/>
/// subclasses or the <see cref="ResultHandler.Facade.Result"/> facade over constructing this directly.
/// </summary>
/// <param name="isSuccessful">Whether the operation succeeded.</param>
/// <param name="status">The outcome status.</param>
/// <param name="title">A short summary of the result.</param>
/// <param name="detail">Optional additional context.</param>
/// <param name="errors">Optional list of individual error messages.</param>
/// <param name="fieldErrors">Optional per-field validation errors, keyed by property name.</param>
[method: JsonConstructor]
public class OperationResult(bool isSuccessful, ResultStatus status, string title, string? detail = null, IReadOnlyList<string>? errors = null, IReadOnlyDictionary<string, IReadOnlyList<string>>? fieldErrors = null)
    : IOperationResult, IResultFailureFactory<OperationResult>
{
    [JsonPropertyName("isSuccessful")]
    public virtual bool IsSuccessful { get; } = isSuccessful;

    [JsonConverter(typeof(ResultStatusJsonConverter))]
    [JsonPropertyName("statusCode")]
    public ResultStatus Status { get; } = status;

    [JsonPropertyName("statusMessage")]
    public string Title { get; } = title;

    [JsonPropertyName("detail")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Detail { get; } = detail;

    [JsonPropertyName("errors")]
    public IReadOnlyList<string> Errors { get; } = errors ?? [];

    [JsonPropertyName("fieldErrors")]
    public IReadOnlyDictionary<string, IReadOnlyList<string>> FieldErrors { get; } = fieldErrors ?? ImmutableDictionary<string, IReadOnlyList<string>>.Empty;

    /// <inheritdoc />
    public static OperationResult Failure(IReadOnlyList<string> errors)
        => new ErrorResult(OperationResultDefaults.ValidationFailedTitle, ResultStatus.UnprocessableContent, errors);

    /// <inheritdoc />
    public static OperationResult Failure(IReadOnlyDictionary<string, IReadOnlyList<string>> fieldErrors)
        => new ErrorResult(OperationResultDefaults.ValidationFailedTitle, ResultStatus.UnprocessableContent, fieldErrors);

    /// <inheritdoc />
    public static OperationResult Failure(string title, string detail, ResultStatus status)
        => new ErrorResult(title, status, detail);

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (OperationResult)obj;
        return IsSuccessful == other.IsSuccessful
            && Status == other.Status
            && Title == other.Title
            && Detail == other.Detail
            && (ReferenceEquals(Errors, other.Errors) || Errors.SequenceEqual(other.Errors))
            && FieldErrorsEqual(other.FieldErrors);
    }

    public override int GetHashCode()
    {
        var hash = default(HashCode);
        hash.Add(IsSuccessful);
        hash.Add(Status);
        hash.Add(Title);
        hash.Add(Detail);
        foreach (var error in Errors)
        {
            hash.Add(error);
        }

        foreach (var pair in FieldErrors)
        {
            hash.Add(pair.Key);
            foreach (var message in pair.Value)
            {
                hash.Add(message);
            }
        }

        return hash.ToHashCode();
    }

    public override string ToString()
        => $"{Status} ({(int)Status.ToHttpStatusCode()}): {Title}";

    private bool FieldErrorsEqual(IReadOnlyDictionary<string, IReadOnlyList<string>> other)
    {
        if (ReferenceEquals(FieldErrors, other))
        {
            return true;
        }

        if (FieldErrors.Count != other.Count)
        {
            return false;
        }

        return FieldErrors.All(pair => other.TryGetValue(pair.Key, out var otherValue) && pair.Value.SequenceEqual(otherValue));
    }
}
