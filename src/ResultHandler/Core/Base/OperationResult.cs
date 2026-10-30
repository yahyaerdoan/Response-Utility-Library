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
/// <param name="errors">Optional error messages by field; <c>""</c> holds messages that belong to no field.</param>
/// <param name="code">Optional machine-readable error code.</param>
[JsonConverter(typeof(OperationResultJsonConverterFactory))]
public class OperationResult(bool isSuccessful, ResultStatus status, string title, string? detail = null, IReadOnlyDictionary<string, IReadOnlyList<string>>? errors = null, string? code = null)
    : IOperationResult, IResultFailureFactory<OperationResult>
{
    /// <inheritdoc cref="IOperationResult.IsSuccessful"/>
    public virtual bool IsSuccessful { get; } = isSuccessful;

    /// <inheritdoc cref="IOperationResult.Status"/>
    public ResultStatus Status { get; } = status;

    /// <inheritdoc cref="IOperationResult.Title"/>
    public string Title { get; } = title;

    /// <inheritdoc cref="IOperationResult.Detail"/>
    public string? Detail { get; } = detail;

    /// <inheritdoc cref="IOperationResult.Errors"/>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Errors { get; } = OperationResultDefaults.Normalize(errors);

    /// <inheritdoc cref="IOperationResult.Code"/>
    public string? Code { get; private set; } = code;

    /// <inheritdoc cref="IOperationResult.Location"/>
    /// <remarks>HTTP metadata, written as the <c>Location</c> header rather than in the body, so it is not serialized.</remarks>
    public string? Location { get; private set; }

    /// <inheritdoc />
    public static OperationResult Failure(IReadOnlyList<string> errors)
        => new ErrorResult(OperationResultDefaults.ValidationFailedTitle, ResultStatus.UnprocessableContent, errors);

    /// <inheritdoc />
    public static OperationResult Failure(IReadOnlyDictionary<string, IReadOnlyList<string>> errors)
        => new ErrorResult(OperationResultDefaults.ValidationFailedTitle, ResultStatus.UnprocessableContent, errors);

    /// <inheritdoc />
    public static OperationResult Failure(string title, string detail, ResultStatus status)
        => new ErrorResult(title, status, detail);

    /// <summary>Whether <paramref name="obj"/> is a result of the same runtime type with equal members, comparing <see cref="IOperationResult.Errors"/> by content.</summary>
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
            && Location == other.Location
            && ErrorsEqual(other.Errors);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = default(HashCode);
        hash.Add(IsSuccessful);
        hash.Add(Status);
        hash.Add(Title);
        hash.Add(Detail);
        hash.Add(Code);
        hash.Add(Location);

        var errorsHash = 0;
        foreach (var (field, messages) in Errors)
        {
            var fieldHash = default(HashCode);
            fieldHash.Add(field);
            foreach (var message in messages)
            {
                fieldHash.Add(message);
            }

            errorsHash ^= fieldHash.ToHashCode();
        }

        hash.Add(errorsHash);
        return hash.ToHashCode();
    }

    /// <summary>The status, its HTTP code and the title, for logs and debugging: <c>NotFound (404): Not Found</c>.</summary>
    public override string ToString()
        => $"{Status} ({(int)Status.ToHttpStatusCode()}): {Title}";

    /// <summary>A shallow copy of this result, same runtime type, with <see cref="Location"/> replaced. Backs <c>WithLocation</c>.</summary>
    internal OperationResult CloneWithLocation(string location)
    {
        var copy = (OperationResult)MemberwiseClone();
        copy.Location = location;
        return copy;
    }

    /// <summary>A shallow copy of this result, same runtime type, with <see cref="Code"/> replaced. Backs <c>WithCode</c> and failure re-projection.</summary>
    internal OperationResult CloneWithCode(string? code)
    {
        var copy = (OperationResult)MemberwiseClone();
        copy.Code = code;
        return copy;
    }

    private bool ErrorsEqual(IReadOnlyDictionary<string, IReadOnlyList<string>> other)
    {
        if (ReferenceEquals(Errors, other))
        {
            return true;
        }

        return Errors.Count == other.Count
            && Errors.All(pair => other.TryGetValue(pair.Key, out var otherMessages) && pair.Value.SequenceEqual(otherMessages));
    }
}
