using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using SampleBatchApi.NETCore;
using System;
using System.Collections.Generic;
using System.Net;


namespace SampleBatchApi.ExceptionHandlers
{
    
    public class ArgumentExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ILogger _logger;

        class ApiResult
        {
            public string Message
            {
                get;
                set;
            }
        }

        public ArgumentExceptionFilter()
        {
        }

        public override void OnException(ExceptionContext context)
        {
            ApiResult apiresult = new ApiResult();
            HttpStatusCode status = HttpStatusCode.InternalServerError;
            string message = context.Exception.Message;

            if (context.Exception is ArgumentException)
            {
                status = HttpStatusCode.BadRequest;
            }

            apiresult.Message = message;

            context.HttpContext.Response.StatusCode = (int)status;

            context.Result = new JsonResult(apiresult);

            if (_logger != null)
            {
                using (_logger.BeginScope(new Dictionary<string, object> { { status.ToString(), context.Exception.ToString() } }))
                {
                    _logger.LogError(context.Result.ToString());
                }
            }


            base.OnException(context);
        }
    }
}