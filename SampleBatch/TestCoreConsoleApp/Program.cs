using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TestCoreConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Application started");

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                /*.AddEnvironmentVariables()
                .AddCommandLine(args)*/
                .Build();

            int iters = configuration.GetValue<int>("Iterations");

            for(int i = 0; i < iters; ++i)
            {
                Console.WriteLine($"Iteration {i}");
                await Task.Delay(1000);
            }

            Console.WriteLine("Application finished");
        }
    }
}
