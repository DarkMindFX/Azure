using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;

namespace SampleBatchApi.ExceptionHandlers
{
    
    public class ArgumentExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            HttpStatusCode status = HttpStatusCode.InternalServerError;
            string message = String.Empty;

            if(actionExecutedContext.Exception.GetType() == typeof(ArgumentException))
            {
                status = HttpStatusCode.BadRequest;
                message = actionExecutedContext.Exception.Message;
            }

            actionExecutedContext.Response = new HttpResponseMessage()
            {
                Content = new StringContent(message, System.Text.Encoding.UTF8, "text/plain"),
                StatusCode = status
            };


            base.OnException(actionExecutedContext);
        }
    }
}