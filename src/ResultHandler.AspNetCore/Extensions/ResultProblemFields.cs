namespace ResultHandler.AspNetCore.Extensions;

/// <summary>The extension members failed results add to Problem Details, for code that reads or completes them (for example in <c>CustomizeProblemDetails</c>).</summary>
public static class ResultProblemFields
{
    /// <summary>The failure's error code, from <c>WithCode</c>.</summary>
    public const string Code = "code";

    /// <summary>The identifier of the request that failed.</summary>
    public const string TraceId = "traceId";
}
