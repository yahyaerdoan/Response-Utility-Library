using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace ResultHandler.AspNetCore.ErrorCodes;

/// <summary>Where <see cref="ErrorCodeCatalog"/> finds codes, where their documentation is served, and whether failed responses link to it.</summary>
public sealed class ErrorCodeCatalogOptions
{
    private readonly List<Assembly> _assemblies = [typeof(ErrorCodeCatalogOptions).Assembly];

    /// <summary>The assemblies whose <see cref="ResultHandler.ErrorCodes.ErrorCodeAttribute"/> constants make up the catalog. ResultHandler's own
    /// codes are always included.</summary>
    public IReadOnlyList<Assembly> Assemblies => _assemblies;

    /// <summary>Path the catalog is served at by <c>MapErrorCodes</c>: the list at the path, one code at <c>{path}/{code}</c>. Defaults to <c>/errors</c>.</summary>
    public string Path { get; set; } = "/errors";

    /// <summary>Whether a failed response's <c>"type"</c> becomes the absolute URL of its code's documentation, instead of the RFC section of
    /// its status. Only codes in the catalog are linked. Defaults to <see langword="true"/>.</summary>
    public bool LinkProblemTypes { get; set; } = true;

    /// <summary>Adds an assembly whose codes belong in the catalog.</summary>
    public ErrorCodeCatalogOptions AddAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        if (!_assemblies.Contains(assembly))
        {
            _assemblies.Add(assembly);
        }

        return this;
    }

    /// <summary>Adds <paramref name="root"/> and every assembly it references, directly or not, whose name starts with one of
    /// <paramref name="namePrefixes"/>: one call covers an application split over many projects.</summary>
    public ErrorCodeCatalogOptions AddAssemblyTree(Assembly root, params string[] namePrefixes)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(namePrefixes);

        var seen = new HashSet<string>(StringComparer.Ordinal) { root.GetName().Name ?? string.Empty };
        var pending = new Queue<Assembly>([root]);
        while (pending.TryDequeue(out var assembly))
        {
            _ = AddAssembly(assembly);
            var matching = assembly.GetReferencedAssemblies()
                .Where(reference => reference.Name is { } name && namePrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal)) && seen.Add(name));

            foreach (var reference in matching)
            {
                pending.Enqueue(Assembly.Load(reference));
            }
        }

        return this;
    }

    /// <summary>The absolute URL of <paramref name="code"/>'s documentation for a client of <paramref name="request"/>.</summary>
    public string DocumentationUrl(HttpRequest request, string code)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return $"{request.Scheme}://{request.Host}{request.PathBase}{Path.TrimEnd('/')}/{Uri.EscapeDataString(code)}";
    }
}
