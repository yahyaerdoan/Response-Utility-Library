using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;
using ResultHandler.Implementations.Error;

namespace ResultHandler.Facade;

public static partial class Result
{
    /// <summary>Runs every result to completion and merges their outcomes, instead of stopping at the first failure like <c>Ensure</c>/<c>Bind</c> — reports every invalid field at once.</summary>
    /// <returns>Success if every result succeeded, otherwise a 422 whose <see cref="IOperationResult.Errors"/> merges every failure's errors by field; a failure without errors contributes its detail (or title) under the <c>""</c> key. Repeated messages appear once;
    /// if every failure is a 422 with the same code, that code is kept.</returns>
    public static IOperationResult Combine(params IOperationResult[] results)
    {
        ArgumentNullException.ThrowIfNull(results);
        return Combine((IEnumerable<IOperationResult>)results);
    }

    /// <inheritdoc cref="Combine(IOperationResult[])"/>
    public static IOperationResult Combine(IEnumerable<IOperationResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var failures = Failures(results);
        return failures.Count == 0
            ? Success()
            : WithSharedCode(new ErrorResult(ResultTitles.ValidationFailed, ResultStatus.UnprocessableContent, CollectErrors(failures)), failures);
    }

    /// <summary>Combines two data results into a tuple on success, or merges failures the same way <see cref="Combine(IOperationResult[])"/> does.</summary>
    public static IOperationResult<(T1 First, T2 Second)> Combine<T1, T2>(IOperationResult<T1> result1, IOperationResult<T2> result2)
    {
        ArgumentNullException.ThrowIfNull(result1);
        ArgumentNullException.ThrowIfNull(result2);

        if (result1.IsSuccessful && result2.IsSuccessful)
        {
            return Success((result1.Data, result2.Data));
        }

        return CombineFailure<(T1 First, T2 Second)>([result1, result2]);
    }

    /// <inheritdoc cref="Combine{T1, T2}(IOperationResult{T1}, IOperationResult{T2})"/>
    public static IOperationResult<(T1 First, T2 Second, T3 Third)> Combine<T1, T2, T3>(IOperationResult<T1> result1, IOperationResult<T2> result2, IOperationResult<T3> result3)
    {
        ArgumentNullException.ThrowIfNull(result1);
        ArgumentNullException.ThrowIfNull(result2);
        ArgumentNullException.ThrowIfNull(result3);

        if (result1.IsSuccessful && result2.IsSuccessful && result3.IsSuccessful)
        {
            return Success((result1.Data, result2.Data, result3.Data));
        }

        return CombineFailure<(T1 First, T2 Second, T3 Third)>([result1, result2, result3]);
    }

    /// <inheritdoc cref="Combine{T1, T2}(IOperationResult{T1}, IOperationResult{T2})"/>
    public static IOperationResult<(T1 First, T2 Second, T3 Third, T4 Fourth)> Combine<T1, T2, T3, T4>(IOperationResult<T1> result1, IOperationResult<T2> result2, IOperationResult<T3> result3, IOperationResult<T4> result4)
    {
        ArgumentNullException.ThrowIfNull(result1);
        ArgumentNullException.ThrowIfNull(result2);
        ArgumentNullException.ThrowIfNull(result3);
        ArgumentNullException.ThrowIfNull(result4);

        if (result1.IsSuccessful && result2.IsSuccessful && result3.IsSuccessful && result4.IsSuccessful)
        {
            return Success((result1.Data, result2.Data, result3.Data, result4.Data));
        }

        return CombineFailure<(T1 First, T2 Second, T3 Third, T4 Fourth)>([result1, result2, result3, result4]);
    }

    private static OperationDataResult<TData> CombineFailure<TData>(IOperationResult[] results)
    {
        var failures = Failures(results);
        return WithSharedCode(new ErrorDataResult<TData>(ResultTitles.ValidationFailed, ResultStatus.UnprocessableContent, CollectErrors(failures)), failures);
    }

    private static List<IOperationResult> Failures(IEnumerable<IOperationResult> results)
        => [.. results.Where(result => !result.IsSuccessful)];

    /// <summary>Keeps the code all failures share, if they are all 422s.</summary>
    private static TResult WithSharedCode<TResult>(TResult combined, List<IOperationResult> failures)
        where TResult : OperationResult
    {
        var code = failures[0].Code;
        return code is not null && failures.TrueForAll(failure => failure.Code == code && failure.Status == ResultStatus.UnprocessableContent)
            ? (TResult)combined.CloneWithCode(code)
            : combined;
    }

    private static Dictionary<string, IReadOnlyList<string>> CollectErrors(List<IOperationResult> failures)
    {
        var merged = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var result in failures)
        {
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

        existing.AddRange(messages.Where(message => !existing.Contains(message)));
    }
}
