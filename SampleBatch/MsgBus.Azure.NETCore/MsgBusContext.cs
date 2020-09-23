using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using SampleBatch.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsgBus.Azure
{
    class MsgBusContextParams : IMsgBusContextParams
    {
        public MsgBusContextParams()
        {
            Parameters = new Dictionary<string, object>();
            Parameters["StorageAccountName"] = null;
            Parameters["StorageAccountKey"] = null;
            Parameters["MessageQueue"] = null;
        }

        public Dictionary<string, object> Parameters 
        {
            get;
            set;
        }
    }

    [Export("Azure", typeof(IMsgBusContext))]
    public class MsgBusContext : IMsgBusContext
    {
        CloudStorageAccount account;
        CloudQueueClient client;
        CloudQueue queue;


        public void Init(IMsgBusContextParams ctxParams)
        {
            account = createCloudStorageAccount(ctxParams);
            client = account.CreateCloudQueueClient();
            queue = client.GetQueueReference((string)ctxParams.Parameters["MessageQueue"]);
            queue.CreateIfNotExistsAsync();
        }        

        public string Id { get; set; }

        public void PutMessage(MsgBusPayload payload)
        { 
            CloudQueueMessage msg = new CloudQueueMessage(JsonConvert.SerializeObject(payload));
            queue.AddMessageAsync(msg);
        }

        public MsgBusPayload GetNextMessage()
        {
            MsgBusPayload result = null;
            CloudQueueMessage newMessage = queue.GetMessageAsync().Result;
            if(newMessage != null)
            {
                result = JsonConvert.DeserializeObject<MsgBusPayload>(newMessage.AsString);

                if(string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(result.Receiver))
                {
                    queue.DeleteMessageAsync(newMessage);
                }
            }

            return result;
        }

        public IMsgBusContextParams PrepareParams()
        {
            return new MsgBusContextParams();
        }

        #region Support method
        CloudStorageAccount createCloudStorageAccount(IMsgBusContextParams ctxParams)
        {
            String accountName = (string)ctxParams.Parameters["StorageAccountName"];
            String accountKey = (string)ctxParams.Parameters["StorageAccountKey"];
            String connString = String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", accountName, accountKey);
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connString);


            return storageAccount;
        }
        #endregion
    }
}
