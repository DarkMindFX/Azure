using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Common;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleBatchProc
{
    class SampleBatchProcApp
    {
        static void Main(string[] args)
        {
            SampleBatchProcApp batchApp = new SampleBatchProcApp();
            batchApp.Start();
        }

        public void Start()
        {
            CloudStorageAccount storageAccount = createCloudStorageAccount();

            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

            string contName = ConfigurationManager.AppSettings["ContainerInput"];

            CloudBlobContainer cloudBlobContainer = getBlobContainer(cloudBlobClient, contName);

            List<string> files = getInputDataFiles();

            List<ResourceFile> inputFiles = new List<ResourceFile>();
            foreach (string f in files)
            {
                if (!String.IsNullOrEmpty(f))
                {
                    inputFiles.Add(GetResource(f, cloudBlobClient, cloudBlobContainer));
                }
            }

            using (BatchClient client = getBatchClient())
            {



                // Create a Batch pool, VM configuration, Windows Server image
                string poolId = ConfigurationManager.AppSettings["PoolId"];
                int poolNodeCount = 1;
                string poolVMSize = "STANDARD_A1_v2";
                string jobId = "DotNetQuickstartJob";


                Console.WriteLine("Creating pool [{0}]...", poolId);

                ImageReference imageReference = new ImageReference(
                        publisher: "MicrosoftWindowsServer",
                        offer: "WindowsServer",
                        sku: "2012-R2-datacenter-smalldisk",
                        version: "latest");

                VirtualMachineConfiguration virtualMachineConfiguration =
                    new VirtualMachineConfiguration(
                        imageReference: imageReference,
                        nodeAgentSkuId: "batch.node.windows amd64");

                try
                {


                    if (client.PoolOperations.ListPools() != null && 
                        client.PoolOperations.ListPools().Count() > 0 && 
                        client.PoolOperations.ListPools().First(p => p.Id == poolId) != null)
                    {
                        client.PoolOperations.DeletePoolAsync(poolId).Wait();
                    }

                    CloudPool pool = client.PoolOperations.CreatePool(
                             poolId: poolId,
                             targetDedicatedComputeNodes: poolNodeCount,
                             virtualMachineSize: poolVMSize,
                             virtualMachineConfiguration: virtualMachineConfiguration);

                    pool.Commit();
                }
                catch (BatchException be)
                {
                    // Accept the specific error code PoolExists as that is expected if the pool already exists
                    if (be.RequestInformation?.BatchError?.Code == BatchErrorCodeStrings.PoolExists)
                    {
                        Console.WriteLine("The pool {0} already existed when we tried to create it", poolId);
                    }
                    else
                    {
                        throw; // Any other exception is unexpected
                    }
                }
                catch (Exception ex)
                {

                }

                try
                {
                    client.JobOperations.DeleteJob(jobId);

                    CloudJob job = client.JobOperations.CreateJob();
                    job.Id = jobId;
                    job.PoolInformation = new PoolInformation { PoolId = poolId };

                    job.Commit();
                }
                catch (BatchException be)
                {
                    // Accept the specific error code JobExists as that is expected if the job already exists
                    if (be.RequestInformation?.BatchError?.Code == BatchErrorCodeStrings.JobExists)
                    {
                        Console.WriteLine("The job {0} already existed when we tried to create it", jobId);
                    }
                    else
                    {
                        throw; // Any other exception is unexpected
                    }
                }

                // Create a collection to hold the tasks that we'll be adding to the job

                Console.WriteLine("Adding {0} tasks to job [{1}]...", inputFiles.Count, jobId);

                List<CloudTask> tasks = new List<CloudTask>();

                // Create each of the tasks to process one of the input files. 

                for (int i = 0; i < inputFiles.Count; i++)
                {
                    string taskId = String.Format("Task{0}", i);
                    string inputFilename = inputFiles[i].FilePath;
                    string taskCommandLine = String.Format("cmd /c type {0}", inputFilename);

                    CloudTask task = new CloudTask(taskId, taskCommandLine);
                    task.ResourceFiles = new List<ResourceFile> { inputFiles[i] };
                    tasks.Add(task);
                }

                // Add all tasks to the job.
                client.JobOperations.AddTask(jobId, tasks);


                // Monitor task success/failure, specifying a maximum amount of time to wait for the tasks to complete.

                TimeSpan timeout = TimeSpan.FromMinutes(10);
                Console.WriteLine("Monitoring all tasks for 'Completed' state, timeout in {0}...", timeout);

                IEnumerable<CloudTask> addedTasks = client.JobOperations.ListTasks(jobId);

                client.Utilities.CreateTaskStateMonitor().WaitAll(addedTasks, TaskState.Completed, timeout);

                Console.WriteLine("All tasks reached state Completed.");

                // Print task output
                Console.WriteLine();
                Console.WriteLine("Printing task output...");

                IEnumerable<CloudTask> completedtasks = client.JobOperations.ListTasks(jobId);

                Stopwatch timer = new Stopwatch();
                timer.Start();

                foreach (CloudTask task in completedtasks)
                {
                    string nodeId = String.Format(task.ComputeNodeInformation.ComputeNodeId);
                    Console.WriteLine("Task: {0}", task.Id);
                    Console.WriteLine("Node: {0}", nodeId);
                    Console.WriteLine("Standard out:");                    
                }

                // Print out some timing info
                timer.Stop();
                Console.WriteLine();
                Console.WriteLine("Sample end: {0}", DateTime.Now);
                Console.WriteLine("Elapsed time: {0}", timer.Elapsed);

                // Clean up Storage resources
                if (cloudBlobContainer != null)
                {
                    cloudBlobContainer.DeleteIfExistsAsync().Wait();
                    Console.WriteLine("Container [{0}] deleted.", contName);
                }
                else
                {
                    Console.WriteLine("Container [{0}] does not exist, skipping deletion.", contName);
                }

                // Clean up Batch resources (if the user so chooses)
                Console.WriteLine();
                Console.Write("Delete job? [yes] no: ");
                string response = Console.ReadLine().ToLower();
                if (response != "n" && response != "no")
                {
                    client.JobOperations.DeleteJob(jobId);
                }

                Console.Write("Delete pool? [yes] no: ");
                response = Console.ReadLine().ToLower();
                if (response != "n" && response != "no")
                {
                    client.PoolOperations.DeletePool(poolId);
                }
            }


        }

        ResourceFile GetResource(string name, CloudBlobClient client, CloudBlobContainer cloudBlobContainer)
        {
            CloudBlob blob = cloudBlobContainer.GetBlockBlobReference(name);

            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(2),
                Permissions = SharedAccessBlobPermissions.Read
            };

            // Construct the SAS URL for blob
            string sasBlobToken = blob.GetSharedAccessSignature(sasConstraints);
            string blobSasUri = String.Format("{0}{1}", blob.Uri, sasBlobToken);

            return ResourceFile.FromUrl(blobSasUri, name);

        }


        CloudStorageAccount createCloudStorageAccount()
        {
            String accountName = ConfigurationManager.AppSettings["StorageAccountName"];
            String accountKey = ConfigurationManager.AppSettings["StorageAccountKey"];
            String connString = String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", accountName, accountKey);
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connString);

            return storageAccount;
        }

        CloudBlobContainer getBlobContainer(CloudBlobClient blobClient, String name)
        {
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(name);

            blobContainer.CreateIfNotExistsAsync().Wait();

            return blobContainer;
        }

        List<String> getInputDataFiles()
        {
            String sFiles = ConfigurationManager.AppSettings["InputDataFiles"];

            List<String> result = new List<string>();
            result.AddRange(sFiles.Split(new char[] { ';' }));
            return result;
        }

        BatchClient getBatchClient()
        {
            string url = ConfigurationManager.AppSettings["BatchAccountURL"];
            string key = ConfigurationManager.AppSettings["BatchAccountKey"];
            string name = ConfigurationManager.AppSettings["BatchAccountName"];

            BatchSharedKeyCredentials cred = new BatchSharedKeyCredentials(url, name, key);

            BatchClient client = BatchClient.Open(cred);

            return client;
        }

    }
}
