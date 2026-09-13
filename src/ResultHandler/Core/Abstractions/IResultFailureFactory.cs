using ResultHandler.Core.Enums;

namespace ResultHandler.Core.Abstractions;

/// <summary>Lets generic code that only knows <typeparamref name="TSelf"/> (a MediatR pipeline behavior, a gRPC interceptor) build a failure instance and <c>return</c> it, instead of throwing to short-circuit.</summary>
/// <remarks>
/// Per-field validation errors are a separate interface, <see cref="IFieldFailureFactory{TSelf}"/> —
/// adding a member here would break existing implementers. Named, per-status shortcuts (<c>BadRequest</c>,
/// <c>NotFound</c>, ...) live in <see cref="ResultHandler.Functional.ResultFailureFactory"/> instead of
/// being redeclared per implementer.
/// </remarks>
/// <typeparam name="TSelf">The implementing result type itself (CRTP).</typeparam>
public interface IResultFailureFactory<TSelf>
    where TSelf : IOperationResult
{
    /// <summary>Builds a failed <typeparamref name="TSelf"/> from a flat list of validation error messages.</summary>
    static abstract TSelf Failure(IReadOnlyList<string> errors);

    /// <summary>Builds a failed <typeparamref name="TSelf"/> with a custom title/detail/status. Prefer <see cref="ResultHandler.Functional.ResultFailureFactory"/>'s named shortcuts over calling this directly.</summary>
    static abstract TSelf Failure(string title, string detail, ResultStatus status);
}
