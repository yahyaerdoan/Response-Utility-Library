using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace ResultHandler.AspNetCore.OpenApi;

/// <summary>RFC 9457 Problem Details returned when a request is invalid: <c>errors</c> maps each field to its messages, and the <c>""</c> key holds messages not tied to a field.</summary>
/// <remarks>OpenAPI response metadata used by <c>ProducesResultProblems</c>/<c>ProducesResultProblem</c> for 400 and 422. The body is written as an <see cref="HttpValidationProblemDetails"/> with <c>code</c> and <c>traceId</c> extensions, so it has exactly this shape.</remarks>
public sealed class ResultValidationProblemDetails : HttpValidationProblemDetails
{
    /// <summary>Machine-readable code identifying the failure, when the server provides one.</summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>Identifier of the request; include it when reporting a problem.</summary>
    [JsonPropertyName("traceId")]
    public string? TraceId { get; set; }
}
