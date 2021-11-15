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
            var retryInterval = ConfigurationManager.AppSettings.Get("CopernicusSearchInterval");
            if (string.IsNullOrWhiteSpace(retryInterval))
            {
                return;
            }
            
            if (!Enum.TryParse(typeof(SearchIntervalTimes), retryInterval, true, out var interval) || interval == null)
            {
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
