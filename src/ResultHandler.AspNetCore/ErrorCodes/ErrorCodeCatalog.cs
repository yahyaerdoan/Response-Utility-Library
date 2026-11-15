using System.Reflection;
using Microsoft.AspNetCore.WebUtilities;
using ResultHandler.ErrorCodes;
using ResultHandler.Mapping;

namespace ResultHandler.AspNetCore.ErrorCodes;

/// <summary>Every documented error code: the string constants carrying <see cref="ErrorCodeAttribute"/> in the configured assemblies. Registered as a
/// singleton by <c>AddErrorCodeCatalog</c>.</summary>
public sealed class ErrorCodeCatalog
{
    /// <summary>ResultHandler's assemblies; their codes are always listed.</summary>
    internal static readonly Assembly[] BuiltInAssemblies = [typeof(ErrorCodeAttribute).Assembly, typeof(ErrorCodeCatalog).Assembly];

    private readonly SortedDictionary<string, ErrorCodeDescription> _codes = new(StringComparer.Ordinal);

    /// <summary>Reads the codes of <paramref name="assemblies"/>; an app's own definition of a built-in code replaces it.</summary>
    /// <exception cref="InvalidOperationException">Two constants define the same code.</exception>
    public ErrorCodeCatalog(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        var distinct = assemblies.Distinct().ToList();
        AddCodes(distinct.Where(BuiltInAssemblies.Contains), replaceable: null);
        var builtIn = _codes.Keys.ToHashSet(StringComparer.Ordinal);
        AddCodes(distinct.Where(assembly => !BuiltInAssemblies.Contains(assembly)), replaceable: builtIn);
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

    /// <summary>Adds documented codes; only a code in <paramref name="replaceable"/> may be redefined.</summary>
    private void AddCodes(IEnumerable<Assembly> assemblies, HashSet<string>? replaceable)
    {
        foreach (var field in assemblies.SelectMany(TypesOf).SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static)))
        {
            if (!field.IsLiteral || field.GetCustomAttribute<ErrorCodeAttribute>() is not { } attribute || field.GetRawConstantValue() is not string code)
            {
                continue;
            }

            if (_codes.TryAdd(code, Describe(code, attribute)))
            {
                continue;
            }

            if (replaceable?.Remove(code) != true)
            {
                throw new InvalidOperationException($"The error code '{code}' is defined more than once; define each code in one place.");
            }

            _codes[code] = Describe(code, attribute);
        }
    }
}
