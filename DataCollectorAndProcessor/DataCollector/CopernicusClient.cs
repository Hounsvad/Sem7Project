using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DataCollector
{
    public class CopernicusClient
    {
        private readonly HttpClient client = new HttpClient();

        public async Task Execute()
        {
            this.client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(ConfigurationManager.AppSettings.Get("APIAuth"));
            var response = await client.GetAsync("https://apihub.copernicus.eu/apihub/search?q=(footprint:\"Intersects(55,10)\" AND platformname:Sentinel-2 AND ingestiondate:[NOW-1MONTHS TO NOW])");
            client.Timeout = -1;
            
            Console.WriteLine(response.Content);
        }
    }
}