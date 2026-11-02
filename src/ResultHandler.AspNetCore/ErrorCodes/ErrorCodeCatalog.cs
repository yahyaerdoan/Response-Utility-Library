using System.Reflection;
using Microsoft.AspNetCore.WebUtilities;
using ResultHandler.ErrorCodes;
using ResultHandler.Mapping;

namespace ResultHandler.AspNetCore.ErrorCodes;

/// <summary>Every documented error code: the string constants carrying <see cref="ErrorCodeAttribute"/> in the configured assemblies. Registered as a
/// singleton by <c>AddErrorCodeCatalog</c>.</summary>
public sealed class ErrorCodeCatalog
{
    private readonly SortedDictionary<string, ErrorCodeDescription> _codes;

    /// <summary>Reads the codes of <paramref name="assemblies"/>.</summary>
    /// <exception cref="InvalidOperationException">Two constants define the same code.</exception>
    public ErrorCodeCatalog(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        _codes = new SortedDictionary<string, ErrorCodeDescription>(StringComparer.Ordinal);
        foreach (var field in assemblies.Distinct().SelectMany(TypesOf).SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static)))
        {
            if (field.IsLiteral && field.GetCustomAttribute<ErrorCodeAttribute>() is { } attribute && field.GetRawConstantValue() is string code
                && !_codes.TryAdd(code, Describe(code, attribute)))
            {
                throw new InvalidOperationException($"The error code '{code}' is defined more than once; define each code in one place.");
            }
        }
    }

    /// <summary>All codes, ordered by code.</summary>
    public IReadOnlyCollection<ErrorCodeDescription> All => _codes.Values;

    /// <summary>The description of <paramref name="code"/>, or <see langword="null"/> when the catalog has no such code.</summary>
    public ErrorCodeDescription? Find(string code)
    {
        ArgumentNullException.ThrowIfNull(code);
        return _codes.GetValueOrDefault(code);
    }

    private static ErrorCodeDescription Describe(string code, ErrorCodeAttribute attribute)
    {
        var status = (int)attribute.Status.ToHttpStatusCode();
        return new ErrorCodeDescription(code, status, ReasonPhrases.GetReasonPhrase(status), attribute.Description, attribute.General);
    }

    private static IEnumerable<Type> TypesOf(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.OfType<Type>();
        }
    }
}
