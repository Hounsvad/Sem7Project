using System;
using System.Timers;
using System.Configuration;

namespace DataCollector
{
    class Program
    {
        static void Main(string[] args)
        {
            var retryInterval = ConfigurationManager.AppSettings.Get("CopernicusSearchInterval");
            if (string.IsNullOrWhiteSpace(retryInterval))
            {
                return;
            }

            if (!int.TryParse(retryInterval, out var interval))
            {
                return;
            }
            
            Timer checkForTime = new Timer(interval);
            checkForTime.Elapsed += Loop;
            checkForTime.AutoReset = true;
            checkForTime.Start();

            Console.ReadLine();
        }

        private static void Loop(object sender, ElapsedEventArgs e)
        {
        }
    }
}
