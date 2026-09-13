using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Enums;
using ResultHandler.Facade;
using ResultHandler.Implementations.Error;

namespace ResultHandler.Functional;

/// <summary>Async counterparts of <see cref="ResultExtensions"/>, for chaining off a <see cref="Task{TResult}"/> or an async mapper/binder without an intermediate <see langword="await"/>.</summary>
public static partial class ResultExtensions
{
    /// <summary>Awaits <paramref name="resultTask"/>, then reduces it into a single value.</summary>
    public static async Task<TOut> MatchAsync<TOut>(this Task<IOperationResult> resultTask, Func<IOperationResult, TOut> onSuccess, Func<IOperationResult, TOut> onFailure)
        => (await resultTask.ConfigureAwait(false)).Match(onSuccess, onFailure);

    /// <summary>Reduces <paramref name="result"/> into a single value using an async projection.</summary>
    public static Task<TOut> MatchAsync<TOut>(this IOperationResult result, Func<IOperationResult, Task<TOut>> onSuccess, Func<IOperationResult, Task<TOut>> onFailure)
        => result.IsSuccessful ? onSuccess(result) : onFailure(result);

    /// <summary>Awaits <paramref name="resultTask"/>, then reduces it into a single value using an async projection.</summary>
    public static async Task<TOut> MatchAsync<TOut>(this Task<IOperationResult> resultTask, Func<IOperationResult, Task<TOut>> onSuccess, Func<IOperationResult, Task<TOut>> onFailure)
        => await (await resultTask.ConfigureAwait(false)).MatchAsync(onSuccess, onFailure).ConfigureAwait(false);

    /// <summary>Awaits <paramref name="resultTask"/>, then reduces the data result into a single value.</summary>
    public static async Task<TOut> MatchAsync<T, TOut>(this Task<IOperationResult<T>> resultTask, Func<T, TOut> onSuccess, Func<IOperationResult<T>, TOut> onFailure)
        => (await resultTask.ConfigureAwait(false)).Match(onSuccess, onFailure);

    /// <summary>Reduces the data result <paramref name="result"/> into a single value using an async projection.</summary>
    public static Task<TOut> MatchAsync<T, TOut>(this IOperationResult<T> result, Func<T, Task<TOut>> onSuccess, Func<IOperationResult<T>, Task<TOut>> onFailure)
        => result.IsSuccessful ? onSuccess(result.Data) : onFailure(result);

    /// <summary>Awaits <paramref name="resultTask"/>, then reduces the data result into a single value using an async projection.</summary>
    public static async Task<TOut> MatchAsync<T, TOut>(this Task<IOperationResult<T>> resultTask, Func<T, Task<TOut>> onSuccess, Func<IOperationResult<T>, Task<TOut>> onFailure)
        => await (await resultTask.ConfigureAwait(false)).MatchAsync(onSuccess, onFailure).ConfigureAwait(false);

    /// <summary>Awaits <paramref name="resultTask"/>, runs a side effect on success and returns it unchanged, for fluent chaining.</summary>
    public static async Task<IOperationResult> OnSuccessAsync(this Task<IOperationResult> resultTask, Action<IOperationResult> action)
        => (await resultTask.ConfigureAwait(false)).OnSuccess(action);

    /// <summary>Runs an async side effect on success and returns <paramref name="result"/> unchanged, for fluent chaining.</summary>
    public static async Task<IOperationResult> OnSuccessAsync(this IOperationResult result, Func<IOperationResult, Task> action)
    {
        if (result.IsSuccessful)
        {
            await action(result).ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>Awaits <paramref name="resultTask"/>, then runs an async side effect on success and returns it unchanged, for fluent chaining.</summary>
    public static async Task<IOperationResult> OnSuccessAsync(this Task<IOperationResult> resultTask, Func<IOperationResult, Task> action)
        => await (await resultTask.ConfigureAwait(false)).OnSuccessAsync(action).ConfigureAwait(false);

    /// <summary>Awaits <paramref name="resultTask"/>, runs a side effect with the typed data on success and returns it unchanged, for fluent chaining.</summary>
    public static async Task<IOperationResult<T>> OnSuccessAsync<T>(this Task<IOperationResult<T>> resultTask, Action<T> action)
        => (await resultTask.ConfigureAwait(false)).OnSuccess(action);

    /// <summary>Runs an async side effect with the typed data on success and returns <paramref name="result"/> unchanged, for fluent chaining.</summary>
    public static async Task<IOperationResult<T>> OnSuccessAsync<T>(this IOperationResult<T> result, Func<T, Task> action)
    {
        if (result.IsSuccessful)
        {
            await action(result.Data).ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>Awaits <paramref name="resultTask"/>, then runs an async side effect with the typed data on success and returns it unchanged, for fluent chaining.</summary>
    public static async Task<IOperationResult<T>> OnSuccessAsync<T>(this Task<IOperationResult<T>> resultTask, Func<T, Task> action)
        => await (await resultTask.ConfigureAwait(false)).OnSuccessAsync(action).ConfigureAwait(false);

    /// <summary>Awaits <paramref name="resultTask"/>, runs a side effect on failure and returns it unchanged, for fluent chaining.</summary>
    public static async Task<IOperationResult> OnFailureAsync(this Task<IOperationResult> resultTask, Action<IOperationResult> action)
        => (await resultTask.ConfigureAwait(false)).OnFailure(action);

    /// <summary>Runs an async side effect on failure and returns <paramref name="result"/> unchanged, for fluent chaining.</summary>
    public static async Task<IOperationResult> OnFailureAsync(this IOperationResult result, Func<IOperationResult, Task> action)
    {
        if (!result.IsSuccessful)
        {
            await action(result).ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>Awaits <paramref name="resultTask"/>, then runs an async side effect on failure and returns it unchanged, for fluent chaining.</summary>
    public static async Task<IOperationResult> OnFailureAsync(this Task<IOperationResult> resultTask, Func<IOperationResult, Task> action)
        => await (await resultTask.ConfigureAwait(false)).OnFailureAsync(action).ConfigureAwait(false);

    /// <summary>Awaits <paramref name="resultTask"/>, then transforms the success payload, short-circuiting a failure unchanged.</summary>
    public static async Task<IOperationResult<TOut>> MapAsync<T, TOut>(this Task<IOperationResult<T>> resultTask, Func<T, TOut> mapper)
        => (await resultTask.ConfigureAwait(false)).Map(mapper);

    /// <summary>Transforms the success payload of <paramref name="result"/> using an async mapper, short-circuiting a failure unchanged.</summary>
    public static async Task<IOperationResult<TOut>> MapAsync<T, TOut>(this IOperationResult<T> result, Func<T, Task<TOut>> mapper)
        => result.IsSuccessful
            ? new Implementations.Success.SuccessDataResult<TOut>(await mapper(result.Data).ConfigureAwait(false), result.Title, result.Status)
            : result.ToErrorDataResult<TOut>();

    /// <summary>Awaits <paramref name="resultTask"/>, then transforms the success payload using an async mapper, short-circuiting a failure unchanged.</summary>
    public static async Task<IOperationResult<TOut>> MapAsync<T, TOut>(this Task<IOperationResult<T>> resultTask, Func<T, Task<TOut>> mapper)
        => await (await resultTask.ConfigureAwait(false)).MapAsync(mapper).ConfigureAwait(false);

    /// <summary>Awaits <paramref name="resultTask"/>, then chains into another result-returning operation, short-circuiting on failure.</summary>
    public static async Task<IOperationResult<TOut>> BindAsync<T, TOut>(this Task<IOperationResult<T>> resultTask, Func<T, IOperationResult<TOut>> binder)
        => (await resultTask.ConfigureAwait(false)).Bind(binder);

    /// <summary>Chains <paramref name="result"/> into another async result-returning operation, short-circuiting on failure.</summary>
    public static async Task<IOperationResult<TOut>> BindAsync<T, TOut>(this IOperationResult<T> result, Func<T, Task<IOperationResult<TOut>>> binder)
        => result.IsSuccessful
            ? await binder(result.Data).ConfigureAwait(false)
            : result.ToErrorDataResult<TOut>();

    /// <summary>Awaits <paramref name="resultTask"/>, then chains into another async result-returning operation, short-circuiting on failure.</summary>
    public static async Task<IOperationResult<TOut>> BindAsync<T, TOut>(this Task<IOperationResult<T>> resultTask, Func<T, Task<IOperationResult<TOut>>> binder)
        => await (await resultTask.ConfigureAwait(false)).BindAsync(binder).ConfigureAwait(false);

    /// <summary>Awaits <paramref name="resultTask"/>, then turns it into a validation failure when <paramref name="predicate"/> rejects the data, for guard-clause-style chaining.</summary>
    public static async Task<IOperationResult<T>> EnsureAsync<T>(this Task<IOperationResult<T>> resultTask, Func<T, bool> predicate, string errorMessage)
        => (await resultTask.ConfigureAwait(false)).Ensure(predicate, errorMessage);

    /// <summary>Turns <paramref name="result"/> into a validation failure when an async <paramref name="predicate"/> rejects the data, for guard-clause-style chaining.</summary>
    public static async Task<IOperationResult<T>> EnsureAsync<T>(this IOperationResult<T> result, Func<T, Task<bool>> predicate, string errorMessage)
        => result.IsSuccessful && !await predicate(result.Data).ConfigureAwait(false)
            ? new ErrorDataResult<T>(ResultTitles.ValidationFailed, ResultStatus.UnprocessableContent, errorMessage)
            : result;

    /// <summary>Awaits <paramref name="resultTask"/>, then turns it into a validation failure when an async <paramref name="predicate"/> rejects the data, for guard-clause-style chaining.</summary>
    public static async Task<IOperationResult<T>> EnsureAsync<T>(this Task<IOperationResult<T>> resultTask, Func<T, Task<bool>> predicate, string errorMessage)
        => await (await resultTask.ConfigureAwait(false)).EnsureAsync(predicate, errorMessage).ConfigureAwait(false);
}
