using FluentValidation;
using JazeeraBooking.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JazeeraBooking.Api.Middleware;

/// <summary>
/// Runs before every action: for each bound argument (e.g. a [FromBody] request DTO), checks
/// whether a FluentValidation IValidator&lt;T&gt; is registered for its type and, if so, runs
/// it. This means adding validation to a new endpoint is just "write an AbstractValidator next
/// to the DTO" - no per-controller boilerplate calling `validator.ValidateAsync(...)`
/// everywhere, and no risk of a controller shipping without its validator wired up. Failures
/// throw ValidationAppException so GlobalExceptionHandler produces the same 400 shape as
/// service-layer validation failures.
/// </summary>
public class ValidationActionFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationActionFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (_serviceProvider.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext);
            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                throw new ValidationAppException(errors);
            }
        }

        await next();
    }
}
