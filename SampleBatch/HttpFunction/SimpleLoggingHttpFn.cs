using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Functions.DTOs;
using System.Web.Http;

namespace HttpFunction
{
    public static class SimpleLoggingHttpFn
    {
        [FunctionName("SimpleLoggingHttpFn")]

        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Queue("function-queue"), StorageAccount("funstorageaccount000")] ICollector<string> outMessage,
            ILogger log)
        {
            IActionResult result = null;
            log.LogInformation("SimpleLoggingHttpFn: started");

            HttpFunctionResponse response = new HttpFunctionResponse()
            {
                GeneratedDt = DateTime.UtcNow
            };

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrEmpty(requestBody))
            {
                response.Success = false;
                result = new BadRequestErrorMessageResult(JsonConvert.SerializeObject(response));
            }
            else
            {
                HttpFunctionRequest request = JsonConvert.DeserializeObject<HttpFunctionRequest>(requestBody);
                if(request != null)
                {
                    QueueFunctionRequest queueReq = new QueueFunctionRequest()
                    {
                        SentDt = DateTime.UtcNow,
                        Payload = JsonConvert.SerializeObject(request)
                    };
                    // TODO: adding to queue
                    outMessage.Add(JsonConvert.SerializeObject(queueReq));
                    response.Success = true;

                    result = new OkObjectResult(JsonConvert.SerializeObject(response));

                }
                else
                {
                    response.Success = false;
                    result = new BadRequestErrorMessageResult(JsonConvert.SerializeObject(response));
                }
            }
            

            return result;
        }
    }
}
