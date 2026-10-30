using System.Text.Json;
using System.Text.Json.Serialization;
using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;
using ResultHandler.Mapping;

namespace ResultHandler.Serialization;

/// <summary>
/// JSON shape of every <see cref="OperationResult"/>: <c>resultData</c> (data results), <c>isSuccessful</c>, <c>statusCode</c> (the HTTP
/// number), <c>statusMessage</c>, <c>detail</c> and <c>code</c> when set, and <c>errors</c> when there are any, the same field-to-messages map
/// validation Problem Details use (<see cref="IOperationResult.Errors"/>). Property names are fixed regardless of the naming policy.
/// Deserialize as <see cref="OperationResult"/> or <see cref="OperationDataResult{T}"/>.
/// </summary>
internal sealed class OperationResultJsonConverterFactory : JsonConverterFactory
{
    private delegate void DataReader(ref Utf8JsonReader reader, JsonSerializerOptions options);

    public override bool CanConvert(Type typeToConvert) => typeof(OperationResult).IsAssignableFrom(typeToConvert);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = DataTypeOf(typeToConvert) is { } dataType
            ? typeof(DataResultConverter<,>).MakeGenericType(typeToConvert, dataType)
            : typeof(ResultConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)(Activator.CreateInstance(converterType)
            ?? throw new InvalidOperationException($"Could not create the JSON converter for {typeToConvert.Name}."));
    }

    private static Type? DataTypeOf(Type type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(OperationDataResult<>))
            {
                return current.GetGenericArguments()[0];
            }
        }

        return null;
    }

    private static void EnsureReadable(Type requested, Type readable)
    {
        if (requested != readable)
        {
            throw new NotSupportedException($"Deserialize results as {readable.Name.Split('`')[0]} rather than {requested.Name.Split('`')[0]}; the JSON does not say which subclass was written.");
        }
    }

    private readonly record struct ResultParts(
        bool IsSuccessful,
        ResultStatus Status,
        string Title,
        string? Detail,
        string? Code,
        IReadOnlyDictionary<string, IReadOnlyList<string>> Errors);

    private static class ResultJson
    {
        private static readonly ResultStatusJsonConverter _statusConverter = new();

        public static void Write(Utf8JsonWriter writer, OperationResult value, Action<Utf8JsonWriter>? writeData)
        {
            writer.WriteStartObject();
            if (writeData is not null)
            {
                writer.WritePropertyName("resultData");
                writeData(writer);
            }

            writer.WriteBoolean("isSuccessful", value.IsSuccessful);
            writer.WriteNumber("statusCode", (int)value.Status.ToHttpStatusCode());
            writer.WriteString("statusMessage", value.Title);
            if (value.Detail is not null)
            {
                writer.WriteString("detail", value.Detail);
            }

            if (value.Code is not null)
            {
                writer.WriteString("code", value.Code);
            }

            var errors = value.Errors;
            if (errors.Count > 0)
            {
                writer.WriteStartObject("errors");
                foreach (var (field, messages) in errors)
                {
                    writer.WriteStartArray(field);
                    foreach (var message in messages)
                    {
                        writer.WriteStringValue(message);
                    }

                    writer.WriteEndArray();
                }

                writer.WriteEndObject();
            }

            writer.WriteEndObject();
        }

        public static ResultParts Read(ref Utf8JsonReader reader, JsonSerializerOptions options, DataReader readData)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("A result must be a JSON object.");
            }

            var comparison = options.PropertyNameCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            bool isSuccessful = false;
            ResultStatus? status = null;
            string title = string.Empty;
            string? detail = null;
            string? code = null;
            IReadOnlyDictionary<string, IReadOnlyList<string>>? errorMap = null;

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                var name = reader.GetString() ?? string.Empty;
                reader.Read();
                if (Is(name, "resultData", comparison))
                {
                    readData(ref reader, options);
                }
                else if (Is(name, "isSuccessful", comparison))
                {
                    isSuccessful = reader.GetBoolean();
                }
                else if (Is(name, "statusCode", comparison))
                {
                    status = _statusConverter.Read(ref reader, typeof(ResultStatus), options);
                }
                else if (Is(name, "statusMessage", comparison))
                {
                    title = reader.GetString() ?? string.Empty;
                }
                else if (Is(name, "detail", comparison))
                {
                    detail = reader.GetString();
                }
                else if (Is(name, "code", comparison))
                {
                    code = reader.GetString();
                }
                else if (Is(name, "errors", comparison) && reader.TokenType == JsonTokenType.StartObject)
                {
                    errorMap = ReadMap(ref reader);
                }
                else
                {
                    reader.Skip();
                }
            }

            return new ResultParts(
                isSuccessful,
                status ?? throw new JsonException("A result needs a \"statusCode\"."),
                title,
                detail,
                code,
                errorMap ?? new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal));
        }

        private static bool Is(string name, string expected, StringComparison comparison) => string.Equals(name, expected, comparison);

        private static Dictionary<string, IReadOnlyList<string>> ReadMap(ref Utf8JsonReader reader)
        {
            var map = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                var field = reader.GetString() ?? string.Empty;
                reader.Read();
                if (reader.TokenType == JsonTokenType.StartArray)
                {
                    map[field] = ReadList(ref reader);
                }
                else
                {
                    reader.Skip();
                }
            }

            return map;
        }

        private static List<string> ReadList(ref Utf8JsonReader reader)
        {
            var list = new List<string>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.String && reader.GetString() is { } message)
                {
                    list.Add(message);
                }
            }

            return list;
        }
    }

    private sealed class ResultConverter<TResult> : JsonConverter<TResult>
        where TResult : OperationResult
    {
        public override TResult Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            EnsureReadable(typeToConvert, typeof(OperationResult));
            var parts = ResultJson.Read(ref reader, options, static (ref Utf8JsonReader dataReader, JsonSerializerOptions _) => dataReader.Skip());
            return (TResult)new OperationResult(parts.IsSuccessful, parts.Status, parts.Title, parts.Detail, parts.Errors, parts.Code);
        }

        public override void Write(Utf8JsonWriter writer, TResult value, JsonSerializerOptions options)
            => ResultJson.Write(writer, value, writeData: null);
    }

    private sealed class DataResultConverter<TResult, TData> : JsonConverter<TResult>
        where TResult : OperationDataResult<TData>
    {
        public override TResult Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            EnsureReadable(typeToConvert, typeof(OperationDataResult<TData>));
            TData? data = default;
            var parts = ResultJson.Read(ref reader, options, (ref Utf8JsonReader dataReader, JsonSerializerOptions dataOptions) => data = JsonSerializer.Deserialize<TData>(ref dataReader, dataOptions));
            return (TResult)new OperationDataResult<TData>(data, parts.IsSuccessful, parts.Status, parts.Title, parts.Detail, parts.Errors, parts.Code);
        }

        public override void Write(Utf8JsonWriter writer, TResult value, JsonSerializerOptions options)
            => ResultJson.Write(writer, value, dataWriter => JsonSerializer.Serialize(dataWriter, value.Data, options));
    }
}
