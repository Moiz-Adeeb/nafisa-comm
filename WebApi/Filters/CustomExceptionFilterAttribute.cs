using System.Net;
using Application.Exceptions;
using Common.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApi.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger _logger;

        public CustomExceptionFilterAttribute(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger("Exception");
        }

        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is AppValidationException)
            {
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Result = new JsonResult(
                    new ResponseBadRequestViewModel
                    {
                        Errors = ((AppValidationException)context.Exception)
                            .Failures.SelectMany(p => p.Value.Select(x => p.Key + " " + x).ToList())
                            .ToList(),
                        Message = "Invalid Object",
                        Success = false,
                    }
                );
                return;
            }

            if (context.Exception is NotActiveException)
            {
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Result = new JsonResult(
                    new ResponseBadRequestViewModel
                    {
                        Message = context.Exception.Message,
                        Success = false,
                    }
                );
                return;
            }

            if (context.Exception is OperationCanceledException)
            {
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Result = new JsonResult(
                    new OperationCancelledRequestViewModel()
                    {
                        Message = context.Exception.Message,
                        Success = false,
                    }
                );
                return;
            }
            if (context.Exception is EntityIsLockedException)
            {
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Result = new JsonResult(
                    new OperationCancelledRequestViewModel()
                    {
                        Message = context.Exception.Message,
                        Success = false,
                    }
                );
                return;
            }
            if (context.Exception is TokenExpiredException)
            {
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Result = new JsonResult(
                    new UnAuthorizedBadRequestViewModel
                    {
                        Message = context.Exception.Message,
                        IsExpired = true,
                        Success = false,
                    }
                );
                return;
            }
            if (context.Exception is TokenNotFoundException)
            {
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Result = new JsonResult(
                    new UnAuthorizedBadRequestViewModel
                    {
                        Message = context.Exception.Message,
                        IsExpired = false,
                        Success = false,
                    }
                );
                return;
            }

            if (context.Exception is BadRequestException)
            {
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Result = new JsonResult(
                    new ResponseBadRequestViewModel
                    {
                        Errors = ((BadRequestException)context.Exception).Failures,
                        Message = context.Exception.Message,
                        Success = false,
                    }
                );
                return;
            }
            if (context.Exception is ThirdPartyException)
            {
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Result = new JsonResult(
                    new ResponseBadRequestViewModel
                    {
                        Message = context.Exception.Message,
                        Success = false,
                    }
                );
                return;
            }

            var code = HttpStatusCode.InternalServerError;

            if (
                context.Exception is DeleteFailureException
                || context.Exception is NotFoundException
                || context.Exception is AlreadyExistsException
                || context.Exception is CannotDeleteException
                || context.Exception is CannotUpdateException
            )
            {
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Result = new JsonResult(
                    new CustomResponseExceptionViewModel()
                    {
                        Message = context.Exception.Message,
                        Success = false,
                    }
                );
                return;
            }

            if (context.Exception is ThirdPartyException)
            {
                code = HttpStatusCode.InternalServerError;
            }

            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = (int)code;
            if (context.Exception.InnerException == null)
            {
                context.Result = new JsonResult(
                    new ResponseExceptionViewModel
                    {
                        Exception = context.Exception.Message,
                        Message = "Internal Server Error has occured",
                        Success = false,
                    }
                );
            }
            else
            {
                context.Result = new JsonResult(
                    new ResponseExceptionViewModel
                    {
                        Exception = context.Exception.InnerException.Message,
                        Message = "Internal Server Error has occured",
                    }
                );
            }
            WriteLogs(context.HttpContext.Request.Path, context.Exception);
        }

        private void WriteLogs(string path, Exception exception)
        {
            if (exception.InnerException != null)
            {
                _logger.LogError(
                    "Error: {Name} {@Request} " + Environment.NewLine + " {@Stack}",
                    path,
                    exception.Message + " -- " + exception.InnerException.Message,
                    exception.StackTrace
                );
            }
            else
            {
                _logger.LogError(
                    "Error: {Name} {@Request} " + Environment.NewLine + " {@Stack}",
                    path,
                    exception.Message,
                    exception.StackTrace
                );
            }
        }
    }
}
