using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using ResultHandler.AspNetCore.Exceptions;
using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;

namespace ResultHandler.AspNetCore.Extensions;

/// <summary>Gives <c>[ApiController]</c>'s automatic model-state response the same Problem Details as failed results.</summary>
public static class ResultModelStateMvcBuilderExtensions
{
    /// <summary>
    /// Replaces <see cref="ApiBehaviorOptions.InvalidModelStateResponseFactory"/>: field errors are keyed like FluentValidation property names
    /// (<c>"ServiceId"</c>, <c>"Items[0].ServiceId"</c>) instead of JSON paths, JSON errors get fixed messages instead of .NET type names, and the
    /// status is 400 when the request could not be read and 422 when it was read but failed validation attributes.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.Services.AddControllers().AddResultModelStateResponses();
    /// </code>
    /// </example>
    public static IMvcBuilder AddResultModelStateResponses(this IMvcBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = context => ToResult(context.ModelState).ToActionResult(context.HttpContext));
    }

    private static OperationResult ToResult(ModelStateDictionary modelState)
    {
        var entries = modelState
            .Where(entry => entry.Value is { Errors.Count: > 0 })
            .SelectMany(entry => entry.Value?.Errors.Select(error => (entry.Key, Error: error)) ?? [])
            .ToList();

        var jsonEntries = entries.Where(entry => entry.Key.StartsWith('$')).ToList();
        if (jsonEntries.Count > 0)
        {
            entries = jsonEntries;
        }

        var fieldErrors = entries
            .SelectMany(entry => RequestBindingErrors.FromModelState(entry.Key, entry.Error.ErrorMessage))
            .GroupBy(error => error.Field, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, IReadOnlyList<string> (group) => [.. group.Select(error => error.Message).Distinct(StringComparer.Ordinal)], StringComparer.Ordinal);

        var unreadable = jsonEntries.Count > 0 || entries.Exists(entry => entry.Error.Exception is not null);
        return unreadable
            ? new OperationResult(false, ResultStatus.BadRequest, ReasonPhrases.GetReasonPhrase(StatusCodes.Status400BadRequest), null, null, fieldErrors)
            : OperationResult.Failure(fieldErrors);
    }
}
