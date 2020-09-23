using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SampleBatch.Interfaces
{
    public class Session
    {
        public string SessionId
        {
            get;
            set;
        }

        public int UserId
        {
            get;
            set;
        }

        public DateTime OpenedDt
        {
            get;
            set;
        }

        public DateTime ExpiresDt
        {
            get;
            set;
        }

        public bool IsActive
        {
            get;
            set;
        }
    }
}