using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SampleBatch.Interfaces;
using SampleBatchApi.Dto;
using SampleBatchApi.ExceptionHandlers;
using SampleBatchApi.NETCore;
using SampleHeavyCalc;
using System;
using System.Net;
using System.Net.Http;

namespace SampleBatchApi.Controllers
{

    [ArgumentExceptionFilter]
    [ApiController]
    public class HeavyCalcController : ControllerBase
    {
        IMsgBusContext ctxMsgBus = null;
        IConfiguration config = null;

        public HeavyCalcController(IConfiguration config)
        {
            this.config = config;
            ctxMsgBus = prepareMsgBusConext();
            
        }

        [HttpPost]
        [Route("api/v1/[controller]/startheavycalc")]
        public IActionResult StartHeavyCalc(StartHeavyCalcRequest request)
        {
            IActionResult response = null;

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

            response = Ok(JsonConvert.SerializeObject(respContent, Formatting.Indented));

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
                String.Format( config["SessionApi"],
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
            IMsgBusContext ctx = Startup.CompositionContainer.GetExportedValue<IMsgBusContext>(
                                        config["MessageBus"]);

            IMsgBusContextParams ctxParams = ctx.PrepareParams();
            ctxParams.Parameters["StorageAccountName"] = config["StorageAccountName"];
            ctxParams.Parameters["StorageAccountKey"] = config["StorageAccountKey"];
            ctxParams.Parameters["MessageQueue"] = config["MessageQueue"];

            ctx.Init(ctxParams);

            return ctx;
        }
    }
}
