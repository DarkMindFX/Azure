using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Functions.DTOs
{
    public class QueueFunctionRequest
    {
        [JsonProperty(PropertyName = "sent_dt")]
        public DateTime SentDt
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "payload")]
        public string Payload
        {
            get;
            set;
        }
            
    }
}
