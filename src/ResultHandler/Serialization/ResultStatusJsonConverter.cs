using System.Text.Json;
using System.Text.Json.Serialization;
using ResultHandler.Core.Enums;
using ResultHandler.Mapping;

namespace ResultHandler.Serialization;

/// <summary>Serializes <see cref="ResultStatus"/> as its numeric HTTP status code, regardless of the caller's <see cref="JsonSerializerOptions"/>.</summary>
public sealed class ResultStatusJsonConverter : JsonConverter<ResultStatus>
{
    /// <summary>Reads a numeric (or numeric-string) HTTP status code back into a <see cref="ResultStatus"/>.</summary>
    public override ResultStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var numericValue))
        {
            return numericValue.ToResultStatus();
        }

        if (reader.TokenType == JsonTokenType.String && int.TryParse(reader.GetString(), out var parsedValue))
        {
            return parsedValue.ToResultStatus();
        }

        throw new JsonException($"Unable to convert JSON value to {nameof(ResultStatus)}: expected a numeric HTTP status code.");
    }

    /// <summary>Writes <paramref name="value"/> as its numeric HTTP status code.</summary>
    public override void Write(Utf8JsonWriter writer, ResultStatus value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue((int)value.ToHttpStatusCode());
    }
}
