using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;
using ResultHandler.Implementations.Error;

namespace ResultHandler.Facade;

public static partial class Result
{
    /// <summary>Runs every result to completion and merges their outcomes, instead of stopping at the first failure like <c>Ensure</c>/<c>Bind</c> — reports every invalid field at once.</summary>
    /// <returns>Success if every result succeeded, otherwise a 422 whose <see cref="IOperationResult.Errors"/> merges every failure's errors by field; a failure without errors contributes its detail (or title) under the <c>""</c> key.</returns>
    public static IOperationResult Combine(params IOperationResult[] results)
        => Combine((IEnumerable<IOperationResult>)results);

    /// <inheritdoc cref="Combine(IOperationResult[])"/>
    public static IOperationResult Combine(IEnumerable<IOperationResult> results)
    {
        var errors = CollectErrors(results);
        return errors.Count == 0
            ? Success()
            : new ErrorResult(ResultTitles.ValidationFailed, ResultStatus.UnprocessableContent, errors);
    }

    /// <summary>Combines two data results into a tuple on success, or merges failures the same way <see cref="Combine(IOperationResult[])"/> does.</summary>
    public static IOperationResult<(T1 First, T2 Second)> Combine<T1, T2>(IOperationResult<T1> result1, IOperationResult<T2> result2)
    {
        if (result1.IsSuccessful && result2.IsSuccessful)
        {
            return Success((result1.Data, result2.Data));
        }

        return CombineFailure<(T1 First, T2 Second)>(CollectErrors([result1, result2]));
    }

    /// <inheritdoc cref="Combine{T1, T2}(IOperationResult{T1}, IOperationResult{T2})"/>
    public static IOperationResult<(T1 First, T2 Second, T3 Third)> Combine<T1, T2, T3>(IOperationResult<T1> result1, IOperationResult<T2> result2, IOperationResult<T3> result3)
    {
        if (result1.IsSuccessful && result2.IsSuccessful && result3.IsSuccessful)
        {
            return Success((result1.Data, result2.Data, result3.Data));
        }

        return CombineFailure<(T1 First, T2 Second, T3 Third)>(CollectErrors([result1, result2, result3]));
    }

    /// <inheritdoc cref="Combine{T1, T2}(IOperationResult{T1}, IOperationResult{T2})"/>
    public static IOperationResult<(T1 First, T2 Second, T3 Third, T4 Fourth)> Combine<T1, T2, T3, T4>(IOperationResult<T1> result1, IOperationResult<T2> result2, IOperationResult<T3> result3, IOperationResult<T4> result4)
    {
        if (result1.IsSuccessful && result2.IsSuccessful && result3.IsSuccessful && result4.IsSuccessful)
        {
            return Success((result1.Data, result2.Data, result3.Data, result4.Data));
        }

        return CombineFailure<(T1 First, T2 Second, T3 Third, T4 Fourth)>(CollectErrors([result1, result2, result3, result4]));
    }

    private static OperationDataResult<TData> CombineFailure<TData>(IReadOnlyDictionary<string, IReadOnlyList<string>> errors)
        => new ErrorDataResult<TData>(ResultTitles.ValidationFailed, ResultStatus.UnprocessableContent, errors);

    private static Dictionary<string, IReadOnlyList<string>> CollectErrors(IEnumerable<IOperationResult> results)
    {
        var merged = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var result in results)
        {
            if (result.IsSuccessful)
            {
                continue;
            }

            if (result.Errors.Count == 0)
            {
                Add(merged, OperationResultDefaults.GeneralErrorKey, [result.Detail ?? result.Title]);
                continue;
            }

            foreach (var (field, messages) in result.Errors)
            {
                Add(merged, field, messages);
            }
        }

        return merged.ToDictionary(pair => pair.Key, IReadOnlyList<string> (pair) => pair.Value, StringComparer.Ordinal);
    }

    private static void Add(Dictionary<string, List<string>> merged, string field, IEnumerable<string> messages)
    {
        if (!merged.TryGetValue(field, out var existing))
        {
            existing = [];
            merged[field] = existing;
        }

        existing.AddRange(messages);
    }
}
