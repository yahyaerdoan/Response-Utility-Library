using System.Text.Json.Serialization;

namespace ResultHandler.AspNetCore.Envelopes;

/// <summary>Body of a successful request that returns no data: the outcome's status, as a number, and its message.</summary>
/// <remarks>Written by <c>ToEnvelopedResult</c>/<c>ToEnvelopedActionResult</c> and declared for OpenAPI by <c>ProducesEnveloped()</c>, so the documented schema is the body itself. Failures are Problem Details, never an envelope.</remarks>
public class ResultEnvelope
{
    /// <summary>Whether the request succeeded; always <see langword="true"/> in a response body.</summary>
    [JsonPropertyName("isSuccessful")]
    public bool IsSuccessful { get; init; }

    /// <summary>The HTTP status code of the outcome.</summary>
    [JsonPropertyName("statusCode")]
    [JsonNumberHandling(JsonNumberHandling.Strict)]
    public int StatusCode { get; init; }

    /// <summary>A short, human-readable summary of the outcome.</summary>
    [JsonPropertyName("statusMessage")]
    public string StatusMessage { get; init; } = string.Empty;

    /// <summary>Additional context, when the server provides it.</summary>
    [JsonPropertyName("detail")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Detail { get; init; }

    /// <summary>Machine-readable code, when the server provides one.</summary>
    [JsonPropertyName("code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Code { get; init; }
}
