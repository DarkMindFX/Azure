using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SampleBatchApi.Dto;
using System;
using System.Net;
using System.Net.Http;


namespace SampleBatchApi.Controllers
{
    [ApiController]
    public class EchoController : ControllerBase
    {
        private IConfiguration config;
        private readonly ILogger logger;

        public EchoController(IConfiguration config, ILogger<EchoController> logger)
        {
            this.config = config;
            this.logger = logger;
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

            logger.LogInformation($"Echo: '{message}' processed");
            
            return Echo(request);
        }

        [HttpPost]
        [Route("api/v1/[controller]")]
        public IActionResult Echo(EchoRequest request)
        {
            EchoResponse echoResp = new EchoResponse()
            {
                Reponse = request.Message,
                Processed = DateTime.Now
            };

            logger.LogInformation($"Echo: '{request.Message}' processed");

            return Ok(JsonConvert.SerializeObject(echoResp, Formatting.Indented));

        }
    }
}
