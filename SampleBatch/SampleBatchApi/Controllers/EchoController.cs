using Newtonsoft.Json;
using SampleBatchApi.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SampleBatchApi.Controllers
{
    [RoutePrefix("api/v1")]
    public class EchoController : ApiController
    {
        [HttpGet]
        [HttpPost]
        [Route("echo/{message}")]
        public HttpResponseMessage Echo(string message)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent(message);
            return response;
        }

        [HttpPost]
        [Route("echo")]
        public HttpResponseMessage Echo(EchoRequest request)
        {
            EchoResponse echoResp = new EchoResponse()
            {
                Reponse = request.Message
            };

            HttpResponseMessage response = new HttpResponseMessage();
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent(JsonConvert.SerializeObject(echoResp));
            return response;

        }
    }
}
