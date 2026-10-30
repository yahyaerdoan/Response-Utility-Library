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
    public static TResult WithCode<TResult>(this TResult result, string code)
        where TResult : OperationResult
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return (TResult)result.CloneWithCode(code);
    }
}
