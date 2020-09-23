using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net;


namespace SampleBatchApi.ExceptionHandlers
{
    
    public class ArgumentExceptionFilter : ExceptionFilterAttribute
    {
        class ApiResult
        {
            public string Message
            {
                get;
                set;
            }
        }
        public override void OnException(ExceptionContext context)
        {
            ApiResult apiresult = new ApiResult();
            HttpStatusCode status = HttpStatusCode.InternalServerError;
            string message = String.Empty;

            if (context.Exception is ArgumentException)
            {
                status = HttpStatusCode.BadRequest;
                message = context.Exception.Message;
            }

            apiresult.Message = message;

            context.HttpContext.Response.StatusCode = (int)status;

            context.Result = new JsonResult(apiresult);


            base.OnException(context);
        }
    }
}