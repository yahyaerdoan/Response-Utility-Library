using ResultHandler.Core.Enums;

namespace ResultHandler.Core.Abstractions;

/// <summary>Lets generic code that only knows <typeparamref name="TSelf"/> (a MediatR pipeline behavior, a gRPC interceptor) build a failure instance and <c>return</c> it, instead of throwing to short-circuit. Named per-status shortcuts live in <see cref="ResultHandler.Functional.ResultFailureFactory"/>.</summary>
/// <typeparam name="TSelf">The implementing result type itself (CRTP).</typeparam>
public interface IResultFailureFactory<TSelf>
    where TSelf : IOperationResult
{
    /// <summary>Builds a failed <typeparamref name="TSelf"/> from validation messages that belong to no field (written under the <c>""</c> key).</summary>
    static abstract TSelf Failure(IReadOnlyList<string> errors);

    /// <summary>Builds a failed <typeparamref name="TSelf"/> with a custom title/detail/status. Prefer <see cref="ResultHandler.Functional.ResultFailureFactory"/>'s named shortcuts over calling this directly.</summary>
    static abstract TSelf Failure(string title, string detail, ResultStatus status);

    /// <summary>Builds a failed <typeparamref name="TSelf"/> from validation messages keyed by property name.</summary>
    static abstract TSelf Failure(IReadOnlyDictionary<string, IReadOnlyList<string>> errors);
}
