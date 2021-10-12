using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml;
using DataCollector.CopernicusDataStructures;

namespace DataCollector
{
    public class CopernicusClient
    {
        private readonly HttpClient client = new HttpClient();

        private readonly string DataAPI = ConfigurationManager.AppSettings.Get("DataAPI");
        private readonly string APIAuth = ConfigurationManager.AppSettings.Get("APIAuth");
        private readonly string SearchArea = ConfigurationManager.AppSettings.Get("SearchArea");
        private readonly string SearchInterval = ConfigurationManager.AppSettings.Get("CopernicusSearchInterval");
        
        public async Task Execute()
        {
            try
            {
                var searchResult = await GetSearchResult();

                if (!searchResult.FoundResults()) return;

                var titleAndId = searchResult.GetTitleAndIdOfFirstEntry();

                var granuleFolderName = await GetGranuleFolderName(titleAndId);

                var imageId = await GetImageId(titleAndId, granuleFolderName);

                var response = await client.GetAsync($"{DataAPI}/odata/v1/Products('{titleAndId.Item2}')" +
                                                     $"/Nodes('{titleAndId.Item1}.SAFE')/Nodes('GRANULE')/Nodes('{granuleFolderName}')/Nodes('IMG_DATA')" +
                                                     $"/Nodes('R10m')/Nodes('{imageId}')/$value");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.StackTrace);
            }

        }

        private async Task<string> GetImageId((string, Guid) titleAndId, string granuleFolderName)
        {
            string imageId = null;
            var response1 = await client.GetAsync($"{DataAPI}/odata/v1/Products('{titleAndId.Item2}')" +
                                                  $"/Nodes('{titleAndId.Item1}.SAFE')/Nodes('GRANULE')/Nodes('{granuleFolderName}')/Nodes('IMG_DATA')" +
                                                  $"/Nodes('R10m')/Nodes");
            var xml = new XmlDocument();
            xml.LoadXml(await response1.Content.ReadAsStringAsync());
            var imagesIds = xml.GetElementsByTagName("d:Id");
            foreach (XmlNode imageNode in imagesIds)
            {
                if (imageNode.InnerText.Contains("TCI_10m"))
                {
                    imageId = imageNode.InnerText;
                }
            }

            if (imageId == null)
                throw new FileNotFoundException($"{titleAndId.Item1} [{titleAndId.Item2}] did not have an image of the desired type");
            
            Console.WriteLine($"Found imageid for {titleAndId.Item1} [{titleAndId.Item2}] as: {imageId}");

            return imageId;
        }

        private async Task<string> GetGranuleFolderName((string, Guid) titleAndId)
        {
            var response =
                await client.GetAsync(
                    $"{DataAPI}/odata/v1/Products('{titleAndId.Item2}')/Nodes('{titleAndId.Item1}.SAFE')/Nodes('GRANULE')/Nodes");
            var xml = new XmlDocument();
            xml.LoadXml(await response.Content.ReadAsStringAsync());
            var granuleFolderName = xml.GetElementsByTagName("d:Id")[0]?.InnerText;
            Console.WriteLine($"Granule folder name: {granuleFolderName}");
            return granuleFolderName;
        }

        private async Task<SearchResult> GetSearchResult()
        {
            this.client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(APIAuth);
            var response = await client.GetAsync(
                $"{DataAPI}/search?q=(footprint:\"Intersects({SearchArea})\" AND platformname:Sentinel-2 AND" +
                $" processinglevel:Level-2A AND platformserialidentifier:Sentinel-2B AND" +
                $" ingestiondate:[NOW-1{SearchInterval} TO NOW])"); // AND cloudcoverpercentage:[0 TO 50]
            var responseString = await response.Content.ReadAsStringAsync();

            return new SearchResult(responseString);
        }
    }
}