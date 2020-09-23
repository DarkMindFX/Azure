using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SampleBatchApi.Dto
{
    public class StartHeavyCalcRequest
    {
        [JsonProperty(PropertyName = "sessionid")]
        public string SessionId
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "filename")]
        public string FileName
        {
            get;
            set;
        }
    }

    public class StartHeavyCalcResponse
    {
        [JsonProperty(PropertyName = "time_started")]
        public DateTime StartedDt
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "is_started")]
        public bool IsStarted
        {
            get;
            set;
        }
    }
}