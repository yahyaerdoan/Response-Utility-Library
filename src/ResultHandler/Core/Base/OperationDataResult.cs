using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Enums;
using ResultHandler.Implementations.Error;
using ResultHandler.Serialization;

namespace ResultHandler.Core.Base;

/// <summary>Immutable base implementation of <see cref="IOperationResult{T}"/>. Prefer <see cref="Implementations.Success.SuccessDataResult{T}"/>/<see cref="Implementations.Error.ErrorDataResult{T}"/> or the <see cref="ResultHandler.Facade.Result"/> facade over constructing this directly.</summary>
/// <param name="data">The data payload; may be <see langword="null"/> when <paramref name="isSuccessful"/> is <see langword="false"/>.</param>
/// <param name="isSuccessful">Whether the operation succeeded.</param>
/// <param name="status">The outcome status.</param>
/// <param name="title">A short summary of the result.</param>
/// <param name="detail">Optional additional context.</param>
/// <param name="errors">Optional error messages by field; <c>""</c> holds messages that belong to no field.</param>
/// <param name="code">Optional machine-readable error code.</param>
[JsonConverter(typeof(OperationResultJsonConverterFactory))]
public class OperationDataResult<T>([AllowNull] T data, bool isSuccessful, ResultStatus status, string title, string? detail = null, IReadOnlyDictionary<string, IReadOnlyList<string>>? errors = null, string? code = null)
    : OperationResult(isSuccessful, status, title, detail, errors, code), IOperationResult<T>, IResultFailureFactory<OperationDataResult<T>>
{
    [AllowNull]
    private readonly T _data = data;

    /// <inheritdoc cref="IOperationResult{T}.Data"/>
    [MaybeNull]
    public T Data => _data;

    /// <inheritdoc cref="IOperationResult.IsSuccessful"/>
    [MemberNotNullWhen(true, nameof(Data))]
    public override bool IsSuccessful => base.IsSuccessful;

    /// <inheritdoc />
    public static new OperationDataResult<T> Failure(IReadOnlyList<string> errors)
        => new ErrorDataResult<T>(OperationResultDefaults.ValidationFailedTitle, ResultStatus.UnprocessableContent, errors);

    /// <inheritdoc />
    public static new OperationDataResult<T> Failure(IReadOnlyDictionary<string, IReadOnlyList<string>> errors)
        => new ErrorDataResult<T>(OperationResultDefaults.ValidationFailedTitle, ResultStatus.UnprocessableContent, errors);

    /// <inheritdoc />
    public static new OperationDataResult<T> Failure(string title, string detail, ResultStatus status)
        => new ErrorDataResult<T>(title, status, detail);

    /// <summary>Whether <paramref name="obj"/> is a result of the same runtime type with equal members, comparing <see cref="IOperationResult.Errors"/> by content.</summary>
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

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Data);
}
