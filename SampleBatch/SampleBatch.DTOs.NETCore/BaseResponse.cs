using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SampleBatch.DTOs.NETCore
{
    public class BaseResponse
    {
        [JsonProperty(PropertyName = "serv_id")]
        public string ServiceID
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "resp_dt")]
        public DateTime ResponseDt
        {
            get;
            set;
        }
    }
}
