using Newtonsoft.Json;
using SampleBatch.DTOs.NETCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SampleBatchApi.Dto
{
    public class OpenSessionRequest
    {
        [JsonProperty(PropertyName = "user_id")]
        public int UserId { get; set; }

    }

    public class OpenSessionResponse : BaseResponse
    {
        [JsonProperty(PropertyName = "session_id")]
        public string SessionId { get; set; }
    }
}