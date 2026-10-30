using System.Collections.Immutable;
using System.Text.Json.Serialization;
using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Enums;
using ResultHandler.Implementations.Error;
using ResultHandler.Mapping;
using ResultHandler.Serialization;

namespace ResultHandler.Core.Base;

/// <summary>Immutable base implementation of <see cref="IOperationResult"/>. Prefer <see cref="Implementations.Success.SuccessResult"/>/<see cref="Implementations.Error.ErrorResult"/> or the <see cref="ResultHandler.Facade.Result"/> facade over constructing this directly.</summary>
/// <param name="isSuccessful">Whether the operation succeeded.</param>
/// <param name="status">The outcome status.</param>
/// <param name="title">A short summary of the result.</param>
/// <param name="detail">Optional additional context.</param>
/// <param name="errors">Optional list of individual error messages.</param>
public class OperationResult(bool isSuccessful, ResultStatus status, string title, string? detail = null, IReadOnlyList<string>? errors = null)
    : IOperationResult, IResultFailureFactory<OperationResult>, IHasFieldErrors
{
    /// <summary>Sets <paramref name="errors"/> and <paramref name="fieldErrors"/> independently; prefer <see cref="Failure(IReadOnlyDictionary{string, IReadOnlyList{string}})"/> for everyday use.</summary>
    public OperationResult(bool isSuccessful, ResultStatus status, string title, string? detail, IReadOnlyList<string>? errors, IReadOnlyDictionary<string, IReadOnlyList<string>>? fieldErrors)
        : this(isSuccessful, status, title, detail, errors)
    {
        FieldErrors = fieldErrors ?? ImmutableDictionary<string, IReadOnlyList<string>>.Empty;
    }

    /// <summary>Sets every member, including <paramref name="code"/>. Public so AOT source-generated JSON deserializers can call it; prefer the <see cref="ResultHandler.Facade.Result"/> facade plus <c>WithCode</c> for everyday use.</summary>
    [JsonConstructor]
    public OperationResult(bool isSuccessful, ResultStatus status, string title, string? detail, IReadOnlyList<string>? errors, IReadOnlyDictionary<string, IReadOnlyList<string>>? fieldErrors, string? code)
        : this(isSuccessful, status, title, detail, errors, fieldErrors)
    {
        Code = code;
    }

    /// <inheritdoc cref="IOperationResult.IsSuccessful"/>
    [JsonPropertyName("isSuccessful")]
    public virtual bool IsSuccessful { get; } = isSuccessful;

    /// <inheritdoc cref="IOperationResult.Status"/>
    [JsonConverter(typeof(ResultStatusJsonConverter))]
    [JsonPropertyName("statusCode")]
    public ResultStatus Status { get; } = status;

    /// <inheritdoc cref="IOperationResult.Title"/>
    [JsonPropertyName("statusMessage")]
    public string Title { get; } = title;

    /// <inheritdoc cref="IOperationResult.Detail"/>
    [JsonPropertyName("detail")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Detail { get; } = detail;

    /// <inheritdoc cref="IOperationResult.Errors"/>
    [JsonPropertyName("errors")]
    public IReadOnlyList<string> Errors { get; } = errors ?? [];

    /// <inheritdoc cref="IHasFieldErrors.FieldErrors"/>
    [JsonPropertyName("fieldErrors")]
    public IReadOnlyDictionary<string, IReadOnlyList<string>> FieldErrors { get; protected set; } = ImmutableDictionary<string, IReadOnlyList<string>>.Empty;

    /// <inheritdoc cref="IOperationResult.Code"/>
    [JsonPropertyName("code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Code { get; private protected set; }

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
            && Code == other.Code
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
        hash.Add(Code);
        foreach (var error in Errors)
        {
            hash.Add(error);
        }

        var fieldErrorsHash = 0;
        foreach (var pair in FieldErrors)
        {
            var pairHash = default(HashCode);
            pairHash.Add(pair.Key);
            foreach (var message in pair.Value)
            {
                pairHash.Add(message);
            }

            fieldErrorsHash ^= pairHash.ToHashCode();
        }

        hash.Add(fieldErrorsHash);

        return hash.ToHashCode();
    }

    public override string ToString()
        => $"{Status} ({(int)Status.ToHttpStatusCode()}): {Title}";

    /// <summary>A shallow copy of this result, same runtime type, with <see cref="Code"/> replaced. Backs <c>WithCode</c> and failure re-projection.</summary>
    internal OperationResult CloneWithCode(string? code)
    {
        var copy = (OperationResult)MemberwiseClone();
        copy.Code = code;
        return copy;
    }

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
