using ResultHandler.Core.Enums;

namespace ResultHandler.ErrorCodes;

/// <summary>Code for a validation failure.</summary>
/// <example>
/// <code>
/// return OperationResult.Failure(fieldErrors).WithCode(ValidationErrorCodes.Failed);
/// </code>
/// </example>
public static class ValidationErrorCodes
{
    /// <summary>Invalid fields (422); <c>"errors"</c> holds each field's messages.</summary>
    [ErrorCode(ResultStatus.UnprocessableContent, "One or more fields are invalid; \"errors\" maps each field to its messages.", General = true)]
    public const string Failed = "Validation.Failed";
}
