using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Sem7.Input.DataCollector;

namespace DataCollector
{
    class Program
    {
        private static CopernicusClient _copernicusClient = new CopernicusClient();
        private static KafkaClient _kafkaClient = new KafkaClient();
        
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
            Task.Run(async () =>
            {
                var ndvis = await _copernicusClient.Execute();
                await _kafkaClient.Produce(ndvis);
            });
        }
    }
}
