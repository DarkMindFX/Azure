using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleBatch.Interfaces
{
    public interface IMsgBusContextParams
    {
        Dictionary<string, object> Parameters { get; set; }
    }

    public class MsgBusPayload
    {
        public string Payload { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
    }

    public interface IMsgBusContext
    {
        void Init(IMsgBusContextParams ctxParams);

        string Id { get; set; }

        void PutMessage(MsgBusPayload payload);
        MsgBusPayload GetNextMessage();
        IMsgBusContextParams PrepareParams();
    }
}
