using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SampleBatch.Interfaces;
using SampleBatchApi.Dto;
using SampleBatchApi.Models;
using SampleBatchApi.NETCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Net.Http;


namespace SampleBatchApi.Controllers
{
    [ApiController]
    public class SessionController : ControllerBase
    {
        private ISessionContext sessionContext = null;
        private IConfiguration config = null;
        private readonly ILogger logger;

        public SessionController(IConfiguration config, ILogger<SessionController> logger)
        {
            this.config = config;
            sessionContext = Startup.CompositionContainer.GetExportedValue<ISessionContext>("Redis");
            this.logger = logger;
        }

        [HttpGet]
        [Route("api/v1/[controller]/{sessionid:guid?}")]
        public IActionResult GetSession(string sessionid)
        {

            Session session = sessionContext.FindSession(sessionid);
            if (session == null)
            {
                logger.LogInformation($"Session Not Found: {sessionid}");
                return NotFound();
            }
            else
            {
                logger.LogInformation($"Session Found OK: {sessionid}");
                return Ok(JsonConvert.SerializeObject(session, Formatting.Indented));
            }


        }


        [HttpPut]
        [Route("api/v1/[controller]")]
        public IActionResult OpenSession(OpenSessionRequest reqOpenSession)
        {
            IActionResult response = this.OpenSession(reqOpenSession.UserId);

            return response;
        }


        [HttpGet]
        [Route("api/v1/[controller]/{id:int}/open")]
        public IActionResult OpenSession(int id)
        {
            Session newSession = new Session()
            {
                UserId = id,
                OpenedDt = DateTime.Now,
                ExpiresDt = DateTime.Now + TimeSpan.FromHours(1)
            };
            string newSessionId = sessionContext.RegisterSession(newSession);

            IActionResult response = null;

            if (!string.IsNullOrEmpty(newSessionId))
            {
                OpenSessionResponse openResp = new OpenSessionResponse()
                {
                    SessionId = newSessionId
                };

                logger.LogInformation($"Session opened OK: {newSessionId}");

                response = Created("api/v1/session/" + newSessionId, JsonConvert.SerializeObject(openResp));
            }
            else
            {
                logger.LogWarning($"Session opened failed (sessionContext.RegisterSession): {newSessionId}");
                response = StatusCode((int)HttpStatusCode.InternalServerError);
            }

            return response;

        }

        [HttpDelete]
        [Route("api/v1/[controller]/{sessionid:guid}")]
        public IActionResult CloseSession(string sessionid)
        {
            IActionResult response = null; ;
            Session session = sessionContext.FindSession(sessionid);
            if (session != null)
            {
                if (sessionContext.CloseSession(sessionid))
                {
                    logger.LogInformation($"Session closed OK: {sessionid}");
                    response = Ok();
                }
                else
                {
                    logger.LogInformation($"Session closed Failed: {sessionid}");
                    response = StatusCode((int)HttpStatusCode.PreconditionFailed);
                }
            }
            else
            {
                logger.LogInformation($"CloseSession: Session Not Found: {sessionid}");
                response = StatusCode((int)HttpStatusCode.NotFound);
            }

            return response;
        }

    }
}
