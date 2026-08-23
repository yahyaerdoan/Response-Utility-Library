using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Enums;
using ResultHandler.Implementations.Error;

namespace ResultHandler.Core.Base;

/// <summary>
/// Base implementation of <see cref="IOperationResult{T}"/>. Immutable; prefer the
/// <see cref="Implementations.Success.SuccessDataResult{T}"/>/<see cref="Implementations.Error.ErrorDataResult{T}"/>
/// subclasses or the <see cref="ResultHandler.Facade.Result"/> facade over constructing this directly.
/// </summary>
/// <param name="data">The data payload; may be <see langword="null"/> when <paramref name="isSuccessful"/> is <see langword="false"/>.</param>
/// <param name="isSuccessful">Whether the operation succeeded.</param>
/// <param name="status">The outcome status.</param>
/// <param name="title">A short summary of the result.</param>
/// <param name="detail">Optional additional context.</param>
/// <param name="errors">Optional list of individual error messages.</param>
/// <param name="fieldErrors">Optional per-field validation errors, keyed by property name.</param>
[method: JsonConstructor]
public class OperationDataResult<T>([AllowNull] T data, bool isSuccessful, ResultStatus status, string title, string? detail = null, IReadOnlyList<string>? errors = null, IReadOnlyDictionary<string, IReadOnlyList<string>>? fieldErrors = null)
    : OperationResult(isSuccessful, status, title, detail, errors, fieldErrors), IOperationResult<T>, IResultFailureFactory<OperationDataResult<T>>, IFieldFailureFactory<OperationDataResult<T>>
{
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
