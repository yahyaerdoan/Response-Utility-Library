using ResultHandler.Core.Enums;

namespace ResultHandler.ErrorCodes;

/// <summary>
/// Documents an error code on the constant that defines it: the one status it comes with and what it tells a client. A code means one thing,
/// so every failure that carries it should have this status; a different situation gets a different code. <c>ResultHandler.AspNetCore</c>'s
/// <c>ErrorCodeCatalog</c> reads these attributes to list the codes and serve their documentation.
/// </summary>
/// <example>
/// <code>
/// public static class OrderErrorCodes
/// {
///     [ErrorCode(ResultStatus.NotFound, "The order doesn't exist or belongs to another user.")]
///     public const string NotFound = "Order.NotFound";
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class ErrorCodeAttribute(ResultStatus status, string description) : Attribute
{
    /// <summary>The status every failure with this code has.</summary>
    public ResultStatus Status { get; } = status;

    /// <summary>What the code tells a client, and what it can do about it.</summary>
    public string Description { get; } = description;

    /// <summary>Whether any endpoint can answer with the code (authentication, validation, rate limiting, server errors), rather than only the
    /// endpoints of the code's own subject.</summary>
    public bool General { get; set; }
}
