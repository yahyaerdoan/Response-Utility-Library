using System.Text.Json.Serialization;

namespace ResultHandler.AspNetCore.Envelopes;

/// <summary>Body of a successful request that returns data: <see cref="ResultEnvelope"/> plus <c>resultData</c>.</summary>
/// <typeparam name="TData">The data type.</typeparam>
/// <remarks>Written by <c>ToEnvelopedResult</c>/<c>ToEnvelopedActionResult</c> on a data result and declared for OpenAPI by <c>ProducesEnveloped&lt;TData&gt;()</c>.</remarks>
public sealed class ResultEnvelope<TData> : ResultEnvelope
{
    /// <summary>The data the request returns.</summary>
    [JsonPropertyName("resultData")]
    [JsonPropertyOrder(-1)]
    public TData? ResultData { get; init; }
}
