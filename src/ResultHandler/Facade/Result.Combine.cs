using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;

namespace ResultHandler.Facade;

public static partial class Result
{
    /// <summary>Runs every result to completion and merges their outcomes, instead of stopping at the first failure like <c>Ensure</c>/<c>Bind</c> — reports every invalid field at once.</summary>
    /// <returns>Success if every result succeeded, otherwise a failure with every message and every <see cref="IHasFieldErrors.FieldErrors"/> merged by key.</returns>
    public static IOperationResult Combine(params IOperationResult[] results)
        => Combine((IEnumerable<IOperationResult>)results);

    /// <inheritdoc cref="Combine(IOperationResult[])"/>
    public static IOperationResult Combine(IEnumerable<IOperationResult> results)
    {
        CollectErrors(results, out var errors, out var fieldErrors);
        if (errors.Count == 0)
        {
            return Success();
        }

        return fieldErrors.Count > 0
            ? new OperationResult(false, ResultStatus.UnprocessableContent, ResultTitles.ValidationFailed, null, errors, fieldErrors)
            : Invalid(errors);
    }

    /// <summary>Combines two data results into a tuple on success, or merges failures the same way <see cref="Combine(IOperationResult[])"/> does.</summary>
    public static IOperationResult<(T1 First, T2 Second)> Combine<T1, T2>(IOperationResult<T1> result1, IOperationResult<T2> result2)
    {
        if (result1.IsSuccessful && result2.IsSuccessful)
        {
            return Success((result1.Data, result2.Data));
        }

        CollectErrors([result1, result2], out var errors, out var fieldErrors);
        return CombineFailure<(T1 First, T2 Second)>(errors, fieldErrors);
    }

    /// <inheritdoc cref="Combine{T1, T2}(IOperationResult{T1}, IOperationResult{T2})"/>
    public static IOperationResult<(T1 First, T2 Second, T3 Third)> Combine<T1, T2, T3>(IOperationResult<T1> result1, IOperationResult<T2> result2, IOperationResult<T3> result3)
    {
        if (result1.IsSuccessful && result2.IsSuccessful && result3.IsSuccessful)
        {
            return Success((result1.Data, result2.Data, result3.Data));
        }

        CollectErrors([result1, result2, result3], out var errors, out var fieldErrors);
        return CombineFailure<(T1 First, T2 Second, T3 Third)>(errors, fieldErrors);
    }

    /// <inheritdoc cref="Combine{T1, T2}(IOperationResult{T1}, IOperationResult{T2})"/>
    public static IOperationResult<(T1 First, T2 Second, T3 Third, T4 Fourth)> Combine<T1, T2, T3, T4>(IOperationResult<T1> result1, IOperationResult<T2> result2, IOperationResult<T3> result3, IOperationResult<T4> result4)
    {
        if (result1.IsSuccessful && result2.IsSuccessful && result3.IsSuccessful && result4.IsSuccessful)
        {
            return Success((result1.Data, result2.Data, result3.Data, result4.Data));
        }

        CollectErrors([result1, result2, result3, result4], out var errors, out var fieldErrors);
        return CombineFailure<(T1 First, T2 Second, T3 Third, T4 Fourth)>(errors, fieldErrors);
    }

    private static OperationDataResult<TData> CombineFailure<TData>(List<string> errors, IReadOnlyDictionary<string, IReadOnlyList<string>> fieldErrors)
        => fieldErrors.Count > 0
            ? new OperationDataResult<TData>(default, false, ResultStatus.UnprocessableContent, ResultTitles.ValidationFailed, null, errors, fieldErrors)
            : Invalid<TData>(errors);

    private static void CollectErrors(IEnumerable<IOperationResult> results, out List<string> errors, out IReadOnlyDictionary<string, IReadOnlyList<string>> fieldErrors)
    {
        errors = [];
        Dictionary<string, List<string>>? mergedFieldErrors = null;

        foreach (var result in results)
        {
            if (result.IsSuccessful)
            {
                continue;
            }

            if (result.Errors.Count > 0)
            {
                errors.AddRange(result.Errors);
            }
            else
            {
                errors.Add(result.Detail ?? result.Title);
            }

            var resultFieldErrors = result.GetFieldErrors();
            if (resultFieldErrors.Count == 0)
            {
                continue;
            }

            mergedFieldErrors ??= [];
            foreach (var pair in resultFieldErrors)
            {
                if (!mergedFieldErrors.TryGetValue(pair.Key, out var messages))
                {
                    messages = [];
                    mergedFieldErrors[pair.Key] = messages;
                }

                messages.AddRange(pair.Value);
            }
        }

        fieldErrors = mergedFieldErrors?.ToDictionary(pair => pair.Key, IReadOnlyList<string> (pair) => pair.Value)
            ?? new Dictionary<string, IReadOnlyList<string>>();
    }
}
