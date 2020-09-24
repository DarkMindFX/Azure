using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SampleBatchApi.Dto;

namespace SampleBatcpApiClient
{
    class BatchApiClient
    {
        class TaskParams
        {
            public int DelayMs
            {
                get;
                set;
            }

            public int UserId
            {
                get;
                set;
            }
        }
        static void Main(string[] args)
        {
            BatchApiClient app = new BatchApiClient();
            app.Start();
        }

        public void Start()
        {
            int threadsCount = Int32.Parse( ConfigurationManager.AppSettings["ClientThreads"] );
            int delayMs = Int32.Parse(ConfigurationManager.AppSettings["SendDelayMs"]);

            List<Task> tasks = new List<Task>();

            for(int i = 0; i < threadsCount; ++i)
            {
                TaskParams taskParams = new TaskParams();
                taskParams.DelayMs = delayMs + i * 50; // making different delays
                taskParams.UserId = 100 + i;

                Task task = Task.Run( () => TaskEntryPoint(taskParams) );
                tasks.Add(task);
                Console.WriteLine(string.Format("Task {0} started", taskParams.UserId));
            }

            Thread.Sleep(5000);

            Task.WaitAll(tasks.ToArray());

        }

        private void TaskEntryPoint(TaskParams rawData)
        {
            TaskParams taskParams = rawData;

            try
            {
                string sessionId = openSession(taskParams.UserId);
                int c = 10;

                while (c > 0)
                {
                    Thread.Sleep(taskParams.DelayMs);
                    sendNextRequest(sessionId);
                    --c;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(String.Format("Exception in thread userid:{0} : {1}\r\nThread exits", taskParams.UserId, ex.Message));
            }
        }

        void sendNextRequest(string sessionId)
        {
            string[] files = { 
                "samplebatchdata000.txt", 
                "samplebatchdata001.txt",
                "samplebatchdata002.txt",
                "samplebatchdata003.txt",
                "samplebatchdata004.txt",
                "samplebatchdata005.txt"
            };

            Random rnd = new Random();
            int idx = rnd.Next(0, 6);

            HttpClient client = new HttpClient();
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post,
                String.Format(ConfigurationManager.AppSettings["HeavyCalcApi"],"startheavycalc"));

            StartHeavyCalcRequest hcReq = new StartHeavyCalcRequest()
            {
                SessionId = sessionId,
                FileName = files[idx]

            };

            string sContent = JsonConvert.SerializeObject(hcReq);
            msg.Content = new StringContent(sContent, Encoding.UTF8, "application/json");
            

            HttpResponseMessage resp = client.SendAsync(msg).Result;
        }

        string openSession(int userId)
        {
            string sessionId = null;

            HttpClient client = new HttpClient();
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get,
                String.Format(ConfigurationManager.AppSettings["SessionApi"],
                    String.Format("session/{0}/open", userId)
                ));

            HttpResponseMessage resp = client.SendAsync(msg).Result;
            OpenSessionResponse openSessionResp = JsonConvert.DeserializeObject<OpenSessionResponse>(resp.Content.ReadAsStringAsync().Result);

            sessionId = openSessionResp.SessionId;

            return sessionId;

        }
    }
}
