using ResultHandler.Core.Enums;

namespace ResultHandler.Core.Abstractions;

/// <summary>
/// Lets generic code that only knows <typeparamref name="TSelf"/> as a type parameter (a MediatR
/// pipeline behavior, a gRPC interceptor, any short-circuiting middleware) construct a failure
/// instance of <typeparamref name="TSelf"/> without knowing its concrete shape — enabling
/// <c>return</c>-based short-circuiting instead of throwing.
/// </summary>
/// <remarks>
/// One name, <c>Failure</c>, overloaded for the two irreducible failure shapes:
/// <see cref="Failure(IReadOnlyList{string})"/> for a flat validation message list, and
/// <see cref="Failure(string, string, ResultStatus)"/> for a title/detail/status triple. Named,
/// per-status shortcuts (<c>BadRequest</c>, <c>NotFound</c>, ...) are generic helpers over the
/// latter in <see cref="ResultHandler.Functional.ResultFailureFactory"/> — written once there for every
/// implementer, instead of being redeclared as interface members on each one.
/// <para>
/// Per-field validation errors are a separate, later-added interface,
/// <see cref="IFieldFailureFactory{TSelf}"/> — a new member here would be a binary-breaking change
/// for anyone implementing this interface directly, so it was not added to this one.
/// </para>
/// </remarks>
/// <typeparam name="TSelf">The implementing result type itself (CRTP).</typeparam>
public interface IResultFailureFactory<TSelf>
    where TSelf : IOperationResult
{
    /// <summary>Builds a failed <typeparamref name="TSelf"/> for one or more validation error messages.</summary>
    /// <param name="errors">The individual error messages (e.g. one per invalid field).</param>
    /// <returns>A failed result with <see cref="IOperationResult.Errors"/> set to <paramref name="errors"/>.</returns>
    /// <example>
    /// Short-circuiting a MediatR pipeline behavior once validation fails, without knowing the
    /// concrete response type:
    /// <code>
    /// where TResponse : IOperationResult, IResultFailureFactory&lt;TResponse&gt;
    /// ...
    /// if (validationErrors.Length > 0)
    /// {
    ///     return TResponse.Failure(validationErrors);
    /// }
    /// </code>
    /// </example>
    static abstract TSelf Failure(IReadOnlyList<string> errors);

    /// <summary>Builds a failed <typeparamref name="TSelf"/> for an arbitrary <paramref name="status"/>.</summary>
    /// <param name="title">A short, human-readable summary of the failure.</param>
    /// <param name="detail">Additional context for this specific occurrence.</param>
    /// <param name="status">The outcome status to fail with.</param>
    /// <returns>A failed result carrying <paramref name="title"/>, <paramref name="detail"/> and <paramref name="status"/>.</returns>
    /// <example>
    /// <code>
    /// where TResponse : IOperationResult, IResultFailureFactory&lt;TResponse&gt;
    /// ...
    /// return TResponse.Failure("Not Found", $"Brand {id} does not exist.", ResultStatus.NotFound);
    /// </code>
    /// Prefer <see cref="ResultHandler.Functional.ResultFailureFactory"/>'s named shortcuts
    /// (<c>ResultFailureFactory.NotFound&lt;TResponse&gt;(detail)</c>) over calling this overload directly.
    /// </example>
    static abstract TSelf Failure(string title, string detail, ResultStatus status);
}
