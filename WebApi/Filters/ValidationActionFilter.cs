using Application.Exceptions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApi.Filters
{
    public class ValidationActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var modelState = context.ModelState;

            if (!modelState.IsValid)
            {
                List<ValidationFailure> list = new List<ValidationFailure>();

                foreach (var key in modelState.Keys)
                {
                    var errors = modelState[key];
                    foreach (var error in errors.Errors)
                    {
                        list.Add(new ValidationFailure(key, error.ErrorMessage));
                    }
                }

                if (list.All(p => p.PropertyName.ToLower() == "id"))
                {
                    return;
                }
                throw new AppValidationException(list);
            }
        }
    }
}
