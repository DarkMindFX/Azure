using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SampleBatchApi.Dto
{
    public class EchoRequest
    {
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }

    public class EchoResponse
    {
        [JsonProperty(PropertyName = "response")]
        public string Reponse { get; set; }
    }
}