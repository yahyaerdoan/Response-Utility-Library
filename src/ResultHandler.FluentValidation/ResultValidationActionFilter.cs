using Microsoft.AspNetCore.Mvc.Filters;
using ResultHandler.AspNetCore.Extensions;
using ResultHandler.Core.Base;

namespace ResultHandler.FluentValidation;

/// <summary>Validates MVC action arguments with their registered FluentValidation validators, answering 422 validation Problem Details instead of running the action when any fails.</summary>
internal sealed class ResultValidationActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var arguments = context.ActionDescriptor.Parameters
            .Where(parameter => context.ActionArguments.ContainsKey(parameter.Name))
            .Select(parameter => (parameter.ParameterType, context.ActionArguments[parameter.Name]));

        var errors = await RequestValidation.ValidateAsync(context.HttpContext.RequestServices, arguments, context.HttpContext.RequestAborted).ConfigureAwait(false);
        if (errors is not null)
        {
            context.Result = OperationResult.Failure(errors).ToActionResult();
            return;
        }

        await next().ConfigureAwait(false);
    }
}
