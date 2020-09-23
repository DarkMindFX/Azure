using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SampleBatch.Interfaces;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using System.Reflection;

namespace SampleHeavyCalc
{
    class SampleHeavyCalc
    {
        static void Main(string[] args)
        {
            SampleHeavyCalc heavyCalc = new SampleHeavyCalc();
            heavyCalc.Start();
        }

        public CompositionContainer CompositionContainer
        {
            get;
            set;
        }



        public void Start()
        {

            #region composition
            AggregateCatalog catalog = new AggregateCatalog();
            DirectoryCatalog directoryCatalog = new DirectoryCatalog(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            catalog.Catalogs.Add(directoryCatalog);
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(SampleHeavyCalc).Assembly));
            CompositionContainer = new CompositionContainer(catalog);

            CompositionContainer.ComposeParts(this);

            #endregion


            CloudStorageAccount account = createCloudStorageAccount();
            CloudBlobClient client = account.CreateCloudBlobClient();

            CloudBlobContainer container = getBlobContainer(client, ConfigurationManager.AppSettings["ContainerInput"]);

            CalcMessage message = null;

            IMsgBusContext msgBusContext = prepareMsgBusConext();

            do
            {
                message = readNextMessage(msgBusContext);
                if (message != null)
                {
                    string inputFileName = message.InputFileName;

                    List<decimal> inputs = getInput(container, inputFileName);

                    List<decimal> outputs = process(inputs);

                    writeOutput(container, inputFileName, outputs);
                }
            }
            while (message != null);


        }

        private CalcMessage readNextMessage(IMsgBusContext queue)
        {
            CalcMessage message = null;
            int retryCount = Int32.Parse(ConfigurationManager.AppSettings["QueueGetMessageRetry"]);
            int retryWaitSec = Int32.Parse(ConfigurationManager.AppSettings["QueueGetMessageRetryWaitSec"]);

            try
            {
                while (retryCount > 0 && message == null)
                {
                    MsgBusPayload payload = queue.GetNextMessage();
                    --retryCount;
                    if (payload != null)
                    {
                        message = JsonConvert.DeserializeObject<CalcMessage>(payload.Payload);
                    }
                    else
                    {
                        Console.WriteLine(String.Format("{1} No message - waiting: retries left {0}", retryCount, DateTime.UtcNow.ToShortTimeString()));
                        System.Threading.Thread.Sleep(retryWaitSec * 1000);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("{1} Exception while reading message: {0}", ex.Message, DateTime.UtcNow.ToShortTimeString()));
                message = null;
            }

            return message;
        }

        private List<decimal> process(List<decimal> inputs)
        {
            List<decimal> results = new List<decimal>();
            foreach (decimal d in inputs)
            {
                Console.Write("Processing " + d + " -> ");
                decimal result = 0;
                for (int i = 0; i < 10000; ++i)
                {
                    result = d + (decimal)(Math.Sqrt((double)d) * Math.Sin((double)d) / Math.Cos((double)d));
                    result = (decimal)Math.Sqrt(Math.Abs((double)result) / Math.PI);
                }
                results.Add(result);
                Console.WriteLine("Done: " + result);
            }

            return results;
        }

        private void writeOutput(CloudBlobContainer container, string blobName, List<decimal> results)
        {
            string name = blobName + "." + DateTime.UtcNow.ToString().Replace("/", "-").Replace(":", "-") + ".txt";

            CloudBlockBlob blob = container.GetBlockBlobReference(name);
            using (StreamWriter sw = new StreamWriter(blob.OpenWrite()))
            {
                foreach (decimal d in results)
                {
                    sw.WriteLine(d.ToString());
                }
            }

            Console.WriteLine("Results dumped -> " + name);
        }

        private List<decimal> getInput(CloudBlobContainer container, string blobName)
        {
            List<decimal> inputs = new List<decimal>();

            CloudBlob blob = container.GetBlobReference(blobName);

            try
            {
                using (StreamReader sr = new StreamReader(blob.OpenRead()))
                {
                    while (!sr.EndOfStream)
                    {
                        string sNumber = sr.ReadLine();
                        if (!String.IsNullOrEmpty(sNumber))
                        {
                            inputs.Add(Decimal.Parse(sNumber));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return inputs;

        }

        CloudStorageAccount createCloudStorageAccount()
        {
            String accountName = ConfigurationManager.AppSettings["StorageAccountName"];
            String accountKey = ConfigurationManager.AppSettings["StorageAccountKey"];
            String connString = String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", accountName, accountKey);
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connString);

            return storageAccount;
        }

        private IMsgBusContext prepareMsgBusConext()
        {
            IMsgBusContext ctx = this.CompositionContainer.GetExportedValue<IMsgBusContext>(
                                        ConfigurationManager.AppSettings["MessageBus"]);

            IMsgBusContextParams ctxParams = ctx.PrepareParams();
            ctxParams.Parameters["StorageAccountName"] = ConfigurationManager.AppSettings["StorageAccountName"];
            ctxParams.Parameters["StorageAccountKey"] = ConfigurationManager.AppSettings["StorageAccountKey"];
            ctxParams.Parameters["MessageQueue"] = ConfigurationManager.AppSettings["MessageQueue"];

            ctx.Init(ctxParams);

            return ctx;
        }

        CloudBlobContainer getBlobContainer(CloudBlobClient blobClient, String name)
        {
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(name);

            blobContainer.CreateIfNotExistsAsync().Wait();

            return blobContainer;
        }
    }
}
