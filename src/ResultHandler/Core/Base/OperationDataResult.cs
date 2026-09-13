using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Enums;
using ResultHandler.Implementations.Error;

namespace ResultHandler.Core.Base;

/// <summary>Immutable base implementation of <see cref="IOperationResult{T}"/>. Prefer <see cref="Implementations.Success.SuccessDataResult{T}"/>/<see cref="Implementations.Error.ErrorDataResult{T}"/> or the <see cref="ResultHandler.Facade.Result"/> facade over constructing this directly.</summary>
/// <param name="data">The data payload; may be <see langword="null"/> when <paramref name="isSuccessful"/> is <see langword="false"/>.</param>
/// <param name="isSuccessful">Whether the operation succeeded.</param>
/// <param name="status">The outcome status.</param>
/// <param name="title">A short summary of the result.</param>
/// <param name="detail">Optional additional context.</param>
/// <param name="errors">Optional list of individual error messages.</param>
public class OperationDataResult<T>([AllowNull] T data, bool isSuccessful, ResultStatus status, string title, string? detail = null, IReadOnlyList<string>? errors = null)
    : OperationResult(isSuccessful, status, title, detail, errors), IOperationResult<T>, IResultFailureFactory<OperationDataResult<T>>
{
    /// <summary>Sets <paramref name="errors"/> and <paramref name="fieldErrors"/> independently — see the matching constructor on <see cref="OperationResult"/>.</summary>
    [JsonConstructor]
    public OperationDataResult([AllowNull] T data, bool isSuccessful, ResultStatus status, string title, string? detail, IReadOnlyList<string>? errors, IReadOnlyDictionary<string, IReadOnlyList<string>>? fieldErrors)
        : this(data, isSuccessful, status, title, detail, errors)
    {
        FieldErrors = fieldErrors ?? ImmutableDictionary<string, IReadOnlyList<string>>.Empty;
    }

    /// <inheritdoc cref="IOperationResult{T}.Data"/>
    [MaybeNull]
    [JsonPropertyName("resultData")]
    public T Data { get; } = data!;

    /// <inheritdoc cref="IOperationResult.IsSuccessful"/>
    [JsonPropertyName("isSuccessful")]
    [MemberNotNullWhen(true, nameof(Data))]
    public override bool IsSuccessful => base.IsSuccessful;

    /// <inheritdoc />
    public static new OperationDataResult<T> Failure(IReadOnlyList<string> errors)
        => new ErrorDataResult<T>(OperationResultDefaults.ValidationFailedTitle, ResultStatus.UnprocessableContent, errors);

    /// <inheritdoc />
    public static new OperationDataResult<T> Failure(IReadOnlyDictionary<string, IReadOnlyList<string>> fieldErrors)
        => new ErrorDataResult<T>(OperationResultDefaults.ValidationFailedTitle, ResultStatus.UnprocessableContent, fieldErrors);

    /// <inheritdoc />
    public static new OperationDataResult<T> Failure(string title, string detail, ResultStatus status)
        => new ErrorDataResult<T>(title, status, detail);

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (OperationDataResult<T>)obj;
        return base.Equals(other)
            && EqualityComparer<T>.Default.Equals(Data, other.Data);
    }

    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Data);
}
