using ResultHandler.Core.Enums;

namespace ResultHandler.Core.Abstractions;

/// <summary>Lets generic code that only knows <typeparamref name="TSelf"/> (a MediatR pipeline behavior, a gRPC interceptor) build a failure instance and <c>return</c> it, instead of throwing to short-circuit. Named per-status shortcuts live in <see cref="ResultHandler.Functional.ResultFailureFactory"/>.</summary>
/// <typeparam name="TSelf">The implementing result type itself (CRTP).</typeparam>
public interface IResultFailureFactory<TSelf>
    where TSelf : IOperationResult
{
    /// <summary>Builds a failed <typeparamref name="TSelf"/> from a flat list of validation error messages.</summary>
    static abstract TSelf Failure(IReadOnlyList<string> errors);

    /// <summary>Builds a failed <typeparamref name="TSelf"/> with a custom title/detail/status. Prefer <see cref="ResultHandler.Functional.ResultFailureFactory"/>'s named shortcuts over calling this directly.</summary>
    static abstract TSelf Failure(string title, string detail, ResultStatus status);

    /// <summary>Builds a failed <typeparamref name="TSelf"/> from per-field validation errors, keyed by property name. Also populates <see cref="IOperationResult.Errors"/> with the flattened messages.</summary>
    static abstract TSelf Failure(IReadOnlyDictionary<string, IReadOnlyList<string>> fieldErrors);
}
