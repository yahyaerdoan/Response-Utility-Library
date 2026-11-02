namespace ResultHandler.AspNetCore.OpenApi;

/// <summary>What <c>AddResultHandlerDocumentation</c> may assume about the API it documents.</summary>
public sealed class ResultHandlerOpenApiOptions
{
    /// <summary>Whether every failed response carries a <c>"code"</c>, so Problem Details document it as required. Turn it on when every failed
    /// result has <c>WithCode</c>, the exception handler includes codes and bare statuses get one in <c>CustomizeProblemDetails</c>.</summary>
    public bool EveryProblemHasCode { get; set; }
}
