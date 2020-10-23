using Newtonsoft.Json;
using System;

namespace Functions.DTOs
{
    public class HttpFunctionRequest
    {
        [JsonProperty(PropertyName = "id")]
        public string ID
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "message")]
        public string Message
        {
            get;
            set;
        }
    }

    public class HttpFunctionResponse
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "generated_dt")]
        public DateTime GeneratedDt
        {
            get;
            set;
        }
    }
}
