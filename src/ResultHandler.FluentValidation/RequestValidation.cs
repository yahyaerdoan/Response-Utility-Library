using System.Collections.Concurrent;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ResultHandler.FluentValidation;

/// <summary>Finds the registered <see cref="IValidator{T}"/>s for request arguments and runs them, shared by the Minimal API and MVC filters.</summary>
internal static class RequestValidation
{
    private static readonly ConcurrentDictionary<Type, Type> _validatorTypes = new();

    /// <summary>Whether a validator is registered for <paramref name="modelType"/>, checked without resolving one (validators are usually scoped).</summary>
    internal static bool HasValidator(IServiceProvider services, Type modelType)
        => services.GetService<IServiceProviderIsService>()?.IsService(ValidatorTypeFor(modelType)) == true;

    /// <summary>Runs every validator registered for each argument's declared type and merges the failures by field, or returns <see langword="null"/> when all pass.</summary>
    internal static async Task<IReadOnlyDictionary<string, IReadOnlyList<string>>?> ValidateAsync(IServiceProvider requestServices, IEnumerable<(Type Type, object? Value)> arguments, CancellationToken cancellationToken)
    {
        Dictionary<string, List<string>>? errors = null;
        foreach (var (type, value) in arguments)
        {
            if (value is null)
            {
                continue;
            }

            foreach (var validator in requestServices.GetServices(ValidatorTypeFor(type)).OfType<IValidator>())
            {
                var result = await validator.ValidateAsync(new ValidationContext<object>(value), cancellationToken).ConfigureAwait(false);
                if (!result.IsValid)
                {
                    errors ??= new Dictionary<string, List<string>>(StringComparer.Ordinal);
                    Merge(errors, result.ToFieldErrors());
                }
            }
        }

        return errors?.ToDictionary(pair => pair.Key, IReadOnlyList<string> (pair) => pair.Value, StringComparer.Ordinal);
    }

    private static void Merge(Dictionary<string, List<string>> errors, IReadOnlyDictionary<string, IReadOnlyList<string>> fieldErrors)
    {
        foreach (var (field, messages) in fieldErrors)
        {
            if (!errors.TryGetValue(field, out var existing))
            {
                existing = [];
                errors[field] = existing;
            }

            existing.AddRange(messages.Where(message => !existing.Contains(message, StringComparer.Ordinal)));
        }
    }

    private static Type ValidatorTypeFor(Type modelType)
        => _validatorTypes.GetOrAdd(modelType, type => typeof(IValidator<>).MakeGenericType(type));
}
