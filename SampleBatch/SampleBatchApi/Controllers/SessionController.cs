using Newtonsoft.Json;
using SampleBatch.Interfaces;
using SampleBatchApi.Dto;
using SampleBatchApi.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SampleBatchApi.Controllers
{
    [RoutePrefix("api/v1")]
    public class SessionController : EchoController
    {
        ISessionContext sessionContext = null;

        public SessionController()
        {
            sessionContext = WebApiApplication.CompositionContainer.GetExportedValue<ISessionContext>("Redis");
        }

        [HttpGet]
        [Route("session/{sessionid:guid}")]
        public HttpResponseMessage GetSession(string sessionid)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            Session session = sessionContext.FindSession(sessionid);
            if(session == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
            }
            else
            {
                response.StatusCode = HttpStatusCode.OK;
                response.Content = new StringContent(JsonConvert.SerializeObject(session));
            }

            return response;

        }


        [HttpPut]
        [Route("session")]
        public HttpResponseMessage OpenSession(OpenSessionRequest reqOpenSession)
        {
            HttpResponseMessage response = this.OpenSession(reqOpenSession.UserId);

            return response;
        }


        [HttpGet]
        [Route("session/{id:int}/open")]
        public HttpResponseMessage OpenSession(int id)
        {
            Session newSession = new Session()
            {
                UserId = id,
                OpenedDt = DateTime.Now,
                ExpiresDt = DateTime.Now + TimeSpan.FromHours(1)
            };
           string newSessionId = sessionContext.RegisterSession(newSession);

            HttpResponseMessage response = new HttpResponseMessage();

            if (!string.IsNullOrEmpty(newSessionId))
            {
                OpenSessionResponse openResp = new OpenSessionResponse()
                {
                    SessionId = newSessionId
                };
                response.StatusCode = HttpStatusCode.Created;
                response.Content = new StringContent(JsonConvert.SerializeObject(openResp));
            }
            else
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;

        }

        [HttpDelete]
        [Route("session/{sessionid:guid}")]
        public HttpResponseMessage CloseSession(string sessionid)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            Session session = sessionContext.FindSession(sessionid);
            if(session != null)
            {
                if (sessionContext.CloseSession(sessionid))
                {
                    response.StatusCode = HttpStatusCode.OK;
                }
                else
                {
                    response.StatusCode = HttpStatusCode.PreconditionFailed;
                }
            }
            else
            {
                response.StatusCode = HttpStatusCode.NotFound;
            }

            return response;
        }

    }
}
