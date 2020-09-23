using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SampleBatchApi.Dto;

using System.Net;
using System.Net.Http;


namespace SampleBatchApi.Controllers
{
    [ApiController]
    public class EchoController : ControllerBase
    {
        private IConfiguration config;
        public EchoController(IConfiguration config)
        {
            this.config = config;
        }

        [HttpGet]
        [HttpPost]
        [Route("api/v1/[controller]/{message?}")]
        public IActionResult Echo(string message)
        {
            EchoRequest request = new EchoRequest()
            {
                Message = message
            };
            
            return Echo(request);
        }

        [HttpPost]
        [Route("api/v1/[controller]")]
        public IActionResult Echo(EchoRequest request)
        {
            EchoResponse echoResp = new EchoResponse()
            {
                Reponse = request.Message
            };

 
            return Ok(JsonConvert.SerializeObject(echoResp, Formatting.Indented));

        }
    }
}
