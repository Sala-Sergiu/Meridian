using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Meridian.Api.Filters;

// Runs the matching FluentValidation validator for each action argument, so
// controllers stay thin (no validation logic). Invalid input becomes a
// ValidationProblemDetails (RFC 7807) 400 response.
public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var services = context.HttpContext.RequestServices;

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (services.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }

                context.Result = new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState));
                return;
            }
        }

        await next();
    }
}
