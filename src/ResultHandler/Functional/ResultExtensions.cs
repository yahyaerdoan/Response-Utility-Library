using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Enums;
using ResultHandler.Facade;
using ResultHandler.Implementations.Error;
using ResultHandler.Implementations.Success;

namespace ResultHandler.Functional;

/// <summary>Functional-style composition helpers over <see cref="IOperationResult"/> and <see cref="IOperationResult{T}"/>.</summary>
public static partial class ResultExtensions
{
    /// <summary>Reduces a result into a single value depending on whether it succeeded.</summary>
    public static TOut Match<TOut>(this IOperationResult result, Func<IOperationResult, TOut> onSuccess, Func<IOperationResult, TOut> onFailure)
        => result.IsSuccessful ? onSuccess(result) : onFailure(result);

    /// <summary>Reduces a data result into a single value, exposing <see cref="IOperationResult{T}.Data"/> directly on success.</summary>
    public static TOut Match<T, TOut>(this IOperationResult<T> result, Func<T, TOut> onSuccess, Func<IOperationResult<T>, TOut> onFailure)
        => result.IsSuccessful ? onSuccess(result.Data) : onFailure(result);

    /// <summary>Runs a side effect when <paramref name="result"/> is successful and returns it unchanged, for fluent chaining.</summary>
    public static IOperationResult OnSuccess(this IOperationResult result, Action<IOperationResult> action)
    {
        if (result.IsSuccessful)
        {
            action(result);
        }

        return result;
    }

    /// <summary>Runs a side effect with the typed <see cref="IOperationResult{T}.Data"/> when <paramref name="result"/> is successful and returns it unchanged, for fluent chaining.</summary>
    public static IOperationResult<T> OnSuccess<T>(this IOperationResult<T> result, Action<T> action)
    {
        if (result.IsSuccessful)
        {
            action(result.Data);
        }

        return result;
    }

    /// <summary>Runs a side effect when <paramref name="result"/> failed and returns it unchanged, for fluent chaining.</summary>
    public static IOperationResult OnFailure(this IOperationResult result, Action<IOperationResult> action)
    {
        if (!result.IsSuccessful)
        {
            action(result);
        }

        return result;
    }

    /// <summary>Transforms the success payload, short-circuiting a failure into an equivalent <see cref="ErrorDataResult{T}"/>.</summary>
    public static IOperationResult<TOut> Map<T, TOut>(this IOperationResult<T> result, Func<T, TOut> mapper)
        => result.IsSuccessful
            ? new SuccessDataResult<TOut>(mapper(result.Data), result.Title, result.Status)
            : Propagate<TOut>(result);

    /// <summary>Chains into another result-returning operation, short-circuiting on failure.</summary>
    public static IOperationResult<TOut> Bind<T, TOut>(this IOperationResult<T> result, Func<T, IOperationResult<TOut>> binder)
        => result.IsSuccessful
            ? binder(result.Data)
            : Propagate<TOut>(result);

    /// <summary>Chains into an operation that returns no data (for example a command), short-circuiting on failure.</summary>
    public static IOperationResult Bind<T>(this IOperationResult<T> result, Func<T, IOperationResult> binder)
        => result.IsSuccessful
            ? binder(result.Data)
            : Propagate(result);

    /// <summary>Turns a still-successful result into a validation failure when <paramref name="predicate"/> rejects the data, for guard-clause-style chaining.</summary>
    public static IOperationResult<T> Ensure<T>(this IOperationResult<T> result, Func<T, bool> predicate, string errorMessage)
        => result.IsSuccessful && !predicate(result.Data)
            ? new ErrorDataResult<T>(ResultTitles.ValidationFailed, ResultStatus.UnprocessableContent, errorMessage)
            : result;

    /// <summary>Turns a still-successful result into a failure with a custom title/status when <paramref name="predicate"/> rejects the data, for guard-clause-style chaining.</summary>
    public static IOperationResult<T> Ensure<T>(this IOperationResult<T> result, Func<T, bool> predicate, string title, string detail, ResultStatus status)
        => result.IsSuccessful && !predicate(result.Data)
            ? new ErrorDataResult<T>(title, status, detail)
            : result;

    /// <summary>Re-projects a failed <see cref="IOperationResult"/> into the typed <see cref="ErrorDataResult{T}"/> a caller must return, carrying title/status/detail/errors/fieldErrors over unchanged. Only call when <paramref name="failed"/> is not successful.</summary>
    /// <example>
    /// <code>
    /// var duplicateCheck = await _brandRules.NameCannotBeDuplicated(request.Name);
    /// if (!duplicateCheck.IsSuccessful)
    /// {
    ///     return duplicateCheck.ToErrorDataResult&lt;CreatedBrandResponse&gt;();
    /// }
    /// </code>
    /// </example>
    public static ErrorDataResult<T> ToErrorDataResult<T>(this IOperationResult failed)
        => Propagate<T>(failed);

    private static ErrorResult Propagate(IOperationResult failed)
    {
        var fieldErrors = failed.GetFieldErrors();
        if (fieldErrors.Count > 0)
        {
            return new ErrorResult(failed.Title, failed.Status, failed.Errors, fieldErrors);
        }

        if (failed.Errors.Count > 0)
        {
            return new ErrorResult(failed.Title, failed.Status, failed.Errors);
        }

        return failed.Detail is null
            ? new ErrorResult(failed.Title, failed.Status)
            : new ErrorResult(failed.Title, failed.Status, failed.Detail);
    }

    private static ErrorDataResult<TOut> Propagate<TOut>(IOperationResult failed)
    {
        var fieldErrors = failed.GetFieldErrors();
        if (fieldErrors.Count > 0)
        {
            return new ErrorDataResult<TOut>(failed.Title, failed.Status, failed.Errors, fieldErrors);
        }

        if (failed.Errors.Count > 0)
        {
            return new ErrorDataResult<TOut>(failed.Title, failed.Status, failed.Errors);
        }

        return failed.Detail is null
            ? new ErrorDataResult<TOut>(failed.Title, failed.Status)
            : new ErrorDataResult<TOut>(failed.Title, failed.Status, failed.Detail);
    }
}
