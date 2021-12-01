using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace DataCollector
{
    class Program
    {
        private static CopernicusClient _copernicusClient = new CopernicusClient();
        
        static void Main(string[] args)
        {
            var retryInterval = Environment.GetEnvironmentVariable("CopernicusSearchInterval");
            if (string.IsNullOrWhiteSpace(retryInterval))
            {
                Console.WriteLine($"CopernicusSearchInterval is null or whitespace");
                return;
            }
            
            if (!Enum.TryParse(typeof(SearchIntervalTimes), retryInterval, true, out var interval) || interval == null)
            {
                Console.WriteLine($"CopernicusSearchInterval could not be parsed [{retryInterval}]");
                return;
            }
            
            Timer checkForTime = new Timer(Loop, null, 0, (long)(int) interval*1000);

            Console.ReadLine();
        }

        private static void Loop(object sender)
        {
            Task.Run(_copernicusClient.Execute);
        }
    }
}
