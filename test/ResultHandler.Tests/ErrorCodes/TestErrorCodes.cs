using ResultHandler.Core.Enums;
using ResultHandler.ErrorCodes;

namespace ResultHandler.Tests.ErrorCodes;

public static class TestErrorCodes
{
    [ErrorCode(ResultStatus.NotFound, "The widget doesn't exist.")]
    public const string WidgetNotFound = "Widget.NotFound";

    [ErrorCode(ResultStatus.Conflict, "The widget is locked.")]
    public const string WidgetLocked = "Widget.Locked";

    public const string Undocumented = "Widget.Undocumented";
}
