using ResultHandler.Core.Enums;
using ResultHandler.ErrorCodes;

namespace ResultHandler.Tests.ErrorCodes;

/// <summary>An app's own definition of a built-in code.</summary>
public static class TestValidationErrorCodes
{
    [ErrorCode(ResultStatus.UnprocessableContent, "The widget form has invalid fields.", General = true)]
    public const string Failed = "Validation.Failed";
}
