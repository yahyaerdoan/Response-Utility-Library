using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace ResultHandler.AspNetCore.Exceptions;

/// <summary>
/// Turns request binding failures into field errors keyed like FluentValidation property names (<c>"ServiceId"</c>, <c>"Items[0].ServiceId"</c>,
/// <c>""</c> when no field applies), with fixed messages that never echo .NET type names. Body failures use the structured
/// <see cref="JsonException.Path"/>; route, query and header failures are read from ASP.NET Core's binding messages, and anything not recognized
/// yields no field errors, so the response falls back to a plain 400.
/// </summary>
internal static partial class RequestBindingErrors
{
    internal const string InvalidValue = "The value is missing or has an invalid format.";
    internal const string Required = "This field is required.";
    internal const string InvalidJson = "The request body is not valid JSON.";
    internal const string BodyRequired = "A request body is required.";

    /// <summary>The field errors of a failed bind, or <see langword="null"/> when the failure can't be attributed.</summary>
    internal static IReadOnlyDictionary<string, IReadOnlyList<string>>? From(BadHttpRequestException exception)
    {
        if (exception.InnerException is JsonException json)
        {
            return FromJson(json);
        }

        var message = exception.Message;
        if (FailedToBindRegex().Match(message) is { Success: true } failedToBind)
        {
            return Single(ToFieldName(ParameterName(failedToBind)), InvalidValue);
        }

        if (RequiredParameterRegex().Match(message) is { Success: true } required)
        {
            return required.Groups["source"].Value.StartsWith("body", StringComparison.OrdinalIgnoreCase)
                ? Single(string.Empty, BodyRequired)
                : Single(ToFieldName(ParameterName(required)), Required);
        }

        return message.StartsWith("Implicit body inferred for parameter", StringComparison.Ordinal)
            ? Single(string.Empty, BodyRequired)
            : null;
    }

    /// <summary>The field errors for a <see cref="JsonException"/> raised while reading a body: a conversion failure names its field, missing <c>required</c> members each get one, and malformed JSON is a body-level error.</summary>
    internal static IReadOnlyDictionary<string, IReadOnlyList<string>> FromJson(JsonException exception)
    {
        if (MissingRequiredRegex().Match(exception.Message) is { Success: true } missing)
        {
            return RequiredFields(missing)
                .Distinct(StringComparer.Ordinal)
                .ToDictionary(field => field, IReadOnlyList<string> (_) => [Required], StringComparer.Ordinal);
        }

        var isConversion = exception.Message.StartsWith("The JSON value could not be converted", StringComparison.Ordinal);
        return isConversion && FromJsonPath(exception.Path) is { Length: > 0 } field
            ? Single(field, InvalidValue)
            : Single(string.Empty, InvalidJson);
    }

    /// <summary>One MVC model-state error as field errors: JSON body entries (<c>$</c>, <c>$.serviceId</c>) get the same keys and fixed messages as <see cref="FromJson"/>; other entries (route, query, form, validation attributes) keep their message under the key as a field name.</summary>
    internal static IEnumerable<(string Field, string Message)> FromModelState(string key, string errorMessage)
    {
        if (key == "$")
        {
            if (MissingRequiredRegex().Match(errorMessage) is { Success: true } missing)
            {
                return RequiredFields(missing).Select(field => (field, Required));
            }

            return [(string.Empty, InvalidJson)];
        }

        if (key.StartsWith('$'))
        {
            return [(FromJsonPath(key), InvalidValue)];
        }

        return [(ToFieldName(key), string.IsNullOrWhiteSpace(errorMessage) ? InvalidValue : errorMessage)];
    }

    /// <summary>A JSON path such as <c>$.items[0].serviceId</c> or <c>$['unit price']</c> as a field name (<c>Items[0].ServiceId</c>); <c>""</c> for the root.</summary>
    internal static string FromJsonPath(string? path)
    {
        if (string.IsNullOrEmpty(path) || path == "$")
        {
            return string.Empty;
        }

        var dotted = QuotedSegmentRegex().Replace(path, ".$1");
        dotted = dotted.StartsWith("$.", StringComparison.Ordinal) ? dotted[2..] : dotted.TrimStart('$');
        return ToFieldName(dotted);
    }

    /// <summary>Upper-cases the first letter of every dotted segment, matching C# property names: <c>items[0].serviceId</c> becomes <c>Items[0].ServiceId</c>.</summary>
    internal static string ToFieldName(string path)
        => string.Join('.', path.Split('.').Select(segment => segment.Length == 0 ? segment : char.ToUpperInvariant(segment[0]) + segment[1..]));

    private static Dictionary<string, IReadOnlyList<string>> Single(string field, string message)
        => new(StringComparer.Ordinal) { [field] = [message] };

    private static IEnumerable<string> RequiredFields(Match missing)
        => missing.Groups["names"].Value
            .Replace("'", string.Empty, StringComparison.Ordinal)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ToFieldName);

    private static string ParameterName(Match match)
        => match.Groups["signature"].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries)[^1];

    [GeneratedRegex("^Failed to bind parameter \"(?<signature>[^\"]+)\" from ")]
    private static partial Regex FailedToBindRegex();

    [GeneratedRegex("^Required parameter \"(?<signature>[^\"]+)\" was not provided from (?<source>[^.]+)")]
    private static partial Regex RequiredParameterRegex();

    [GeneratedRegex("missing required properties,? including(?: the following)?: (?<names>.+?)\\.?$")]
    private static partial Regex MissingRequiredRegex();

    [GeneratedRegex(@"\['([^']*)'\]")]
    private static partial Regex QuotedSegmentRegex();
}
