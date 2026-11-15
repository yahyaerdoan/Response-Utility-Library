using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Base;

namespace ResultHandler.Facade;

/// <summary>Attaches a machine-readable <see cref="OperationResult.Code"/> to a result.</summary>
public static class ErrorCodeExtensions
{
    /// <summary>Returns a copy of <paramref name="result"/>, same runtime type, with <see cref="OperationResult.Code"/> set to <paramref name="code"/>. Written to Problem Details as the <c>"code"</c> extension.</summary>
    /// <example>
    /// <code>
    /// return Result.Conflict("That slug is already taken.").WithCode("Tenant.SlugTaken");
    /// </code>
    /// </example>
    /// <remarks>Works on any result, including a pipeline's <c>TResponse</c> and <c>Result.Combine</c>'s result.</remarks>
    public static TResult WithCode<TResult>(this TResult result, string code)
        where TResult : class, IOperationResult
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return result is OperationResult operationResult
            ? (TResult)(IOperationResult)operationResult.CloneWithCode(code)
            : throw new ArgumentException($"{result.GetType().Name} is not a ResultHandler result, so it can't carry a code.", nameof(result));
    }
}
