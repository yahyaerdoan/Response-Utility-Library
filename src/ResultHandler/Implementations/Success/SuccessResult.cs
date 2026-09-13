using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;

namespace ResultHandler.Implementations.Success;

/// <summary>A successful <see cref="OperationResult"/> (<c>IsSuccessful</c> is always <see langword="true"/>).</summary>
public class SuccessResult : OperationResult
{
    /// <summary>Default success: status <see cref="ResultStatus.Ok"/>, title "Operation completed successfully.".</summary>
    public SuccessResult()
        : base(true, ResultStatus.Ok, OperationResultDefaults.SuccessTitle)
    {
    }

    /// <summary>Success with a custom title, status <see cref="ResultStatus.Ok"/>.</summary>
    public SuccessResult(string title)
        : base(true, ResultStatus.Ok, title)
    {
    }

    /// <summary>Success with a custom title and status.</summary>
    public SuccessResult(string title, ResultStatus status)
        : base(true, status, title)
    {
    }
}
