using Newtonsoft.Json;
using SampleBatch.Interfaces;
using SampleBatchApi.Dto;
using SampleBatchApi.ExceptionHandlers;
using SampleHeavyCalc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SampleBatchApi.Controllers
{
    [RoutePrefix("api/v1")]
    [ArgumentExceptionFilter]
    public class HeavyCalcController : ApiController
    {
        IMsgBusContext ctxMsgBus = null;

        public HeavyCalcController()
        {
            ctxMsgBus = prepareMsgBusConext();
        }

        [HttpPost]
        [Route("startheavycalc")]
        public HttpResponseMessage StartHeavyCalc(StartHeavyCalcRequest request)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            validateStrParam("sessionid", request.SessionId);
            validateStrParam("filename", request.FileName);

            if(!isValidSession(request.SessionId))
            {
                throw new ArgumentException(string.Format("Invalid session toekn: {0}", request.SessionId));
            }

            sendStartMessage(request.FileName);

            StartHeavyCalcResponse respContent = new StartHeavyCalcResponse()
            {
                IsStarted = true,
                StartedDt = DateTime.UtcNow
            };

            response.Content = new StringContent(JsonConvert.SerializeObject(respContent));

            return response;
        }

        private void validateStrParam(string name, string value)
        {
            if(string.IsNullOrEmpty(value))
            {
                throw new ArgumentException(string.Format("Parameters {0} has invalid value: {1}", name, value));
            }
        }

        bool isValidSession(string sessionId)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get,
                String.Format( ConfigurationManager.AppSettings["SessionApi"],
                    String.Format("session/{0}", sessionId)
                ));
            
            HttpResponseMessage resp = client.SendAsync(msg).Result;

            return resp.StatusCode == HttpStatusCode.OK;

        }

        private void sendStartMessage(string fileName)
        {
            CalcMessage calcMessage = new CalcMessage();
            calcMessage.InputFileName = fileName;

            MsgBusPayload msgBusPayload = new MsgBusPayload();
            msgBusPayload.Sender = "HeavyCalcController";
            msgBusPayload.Payload = JsonConvert.SerializeObject(calcMessage);

            ctxMsgBus.PutMessage(msgBusPayload);
        }

        private IMsgBusContext prepareMsgBusConext()
        {
            IMsgBusContext ctx = WebApiApplication.CompositionContainer.GetExportedValue<IMsgBusContext>(
                                        ConfigurationManager.AppSettings["MessageBus"]);

            IMsgBusContextParams ctxParams = ctx.PrepareParams();
            ctxParams.Parameters["StorageAccountName"] = ConfigurationManager.AppSettings["StorageAccountName"];
            ctxParams.Parameters["StorageAccountKey"] = ConfigurationManager.AppSettings["StorageAccountKey"];
            ctxParams.Parameters["MessageQueue"] = ConfigurationManager.AppSettings["MessageQueue"];

            ctx.Init(ctxParams);

            return ctx;
        }
    }
}
