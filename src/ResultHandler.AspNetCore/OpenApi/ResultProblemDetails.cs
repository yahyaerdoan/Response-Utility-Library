using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace ResultHandler.AspNetCore.OpenApi;

/// <summary>RFC 9457 Problem Details returned when a request fails, with an optional error code and the request's trace id.</summary>
/// <remarks>OpenAPI response metadata used by <c>ProducesResultProblems</c>/<c>ProducesResultProblem</c>. The body is written from a <see cref="ProblemDetails"/> whose extensions carry <c>code</c> and <c>traceId</c>, so it has exactly this shape.</remarks>
public sealed class ResultProblemDetails : ProblemDetails
{
    /// <summary>Machine-readable code identifying the failure, when the server provides one.</summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>Identifier of the request; include it when reporting a problem.</summary>
    [JsonPropertyName("traceId")]
    public string? TraceId { get; set; }
}
