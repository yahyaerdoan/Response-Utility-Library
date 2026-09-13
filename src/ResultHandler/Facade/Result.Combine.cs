using ResultHandler.Core.Abstractions;

namespace ResultHandler.Facade;

public static partial class Result
{
    /// <summary>Runs every result to completion and merges their outcomes, instead of short-circuiting on the first failure like <c>Ensure</c>/<c>Bind</c> — use this to report every invalid field on a form at once, not just the first.</summary>
    /// <returns><see cref="Result.Success()"/> if every result succeeded, otherwise <see cref="Result.Invalid(IReadOnlyList{string})"/> with every failure's messages concatenated in order.</returns>
    public static IOperationResult Combine(params IOperationResult[] results)
        => Combine((IEnumerable<IOperationResult>)results);

    /// <inheritdoc cref="Combine(IOperationResult[])"/>
    public static IOperationResult Combine(IEnumerable<IOperationResult> results)
    {
        CollectErrors(results, out var errors);
        return errors.Count > 0 ? Invalid(errors) : Success();
    }

    /// <summary>
    /// Combines two data results: if both succeeded, returns their data as a tuple; otherwise merges
    /// every failing result's messages the same way <see cref="Combine(IOperationResult[])"/> does.
    /// </summary>
    public static IOperationResult<(T1 First, T2 Second)> Combine<T1, T2>(IOperationResult<T1> result1, IOperationResult<T2> result2)
    {
        if (result1.IsSuccessful && result2.IsSuccessful)
        {
            return Success((result1.Data, result2.Data));
        }

        CollectErrors([result1, result2], out var errors);
        return Invalid<(T1 First, T2 Second)>(errors);
    }

    /// <inheritdoc cref="Combine{T1, T2}(IOperationResult{T1}, IOperationResult{T2})"/>
    public static IOperationResult<(T1 First, T2 Second, T3 Third)> Combine<T1, T2, T3>(IOperationResult<T1> result1, IOperationResult<T2> result2, IOperationResult<T3> result3)
    {
        if (result1.IsSuccessful && result2.IsSuccessful && result3.IsSuccessful)
        {
            return Success((result1.Data, result2.Data, result3.Data));
        }

        CollectErrors([result1, result2, result3], out var errors);
        return Invalid<(T1 First, T2 Second, T3 Third)>(errors);
    }

    /// <inheritdoc cref="Combine{T1, T2}(IOperationResult{T1}, IOperationResult{T2})"/>
    public static IOperationResult<(T1 First, T2 Second, T3 Third, T4 Fourth)> Combine<T1, T2, T3, T4>(IOperationResult<T1> result1, IOperationResult<T2> result2, IOperationResult<T3> result3, IOperationResult<T4> result4)
    {
        if (result1.IsSuccessful && result2.IsSuccessful && result3.IsSuccessful && result4.IsSuccessful)
        {
            return Success((result1.Data, result2.Data, result3.Data, result4.Data));
        }

        CollectErrors([result1, result2, result3, result4], out var errors);
        return Invalid<(T1 First, T2 Second, T3 Third, T4 Fourth)>(errors);
    }

    private static void CollectErrors(IEnumerable<IOperationResult> results, out List<string> errors)
    {
        errors = [];
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
        }
    }
}
