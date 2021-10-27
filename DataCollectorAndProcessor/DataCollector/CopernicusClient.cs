using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml;
using DataCollector.CopernicusDataStructures;
using DataCollectorAndProcessor;
using Sem7.Input.Common;
using Sem7.Input.Processor;

namespace DataCollector
{
    public class CopernicusClient
    {
        private HttpClient _client = new HttpClient();
        private IImageProcessor _processor = new ImageProcessor();

        private readonly string DataAPI = ConfigurationManager.AppSettings.Get("DataAPI");
        private readonly string APIAuth = ConfigurationManager.AppSettings.Get("APIAuth");
        private readonly string SearchArea = ConfigurationManager.AppSettings.Get("SearchArea");
        private readonly string SearchInterval = ConfigurationManager.AppSettings.Get("CopernicusSearchInterval");

        public async Task<NDVIPixel[]> Execute()
        {
            try
            {
                var searchResult = await GetSearchResult();

                if (!searchResult.FoundResults()) return null;

                var titleAndId = searchResult.GetTitleAndIdOfFirstEntry();

                var boundingCoordinates = await GetBoundingCoordinates(titleAndId);
                var polygon = await GetPolygon(titleAndId);

                var granuleFolderName = await GetGranuleFolderName(titleAndId);

                var imageB04Id = await GetImageId(titleAndId, granuleFolderName, ImageTypes.B04.ToString());
                var imageB08Id = await GetImageId(titleAndId, granuleFolderName, ImageTypes.B08.ToString());

                var imageStreamB04 = await GetImageStream(titleAndId, granuleFolderName, imageB04Id);
                var imageStreamB08 = await GetImageStream(titleAndId, granuleFolderName, imageB08Id);

                var bitmapB04 = ImageParser.ParseImageStream(imageStreamB04);
                var bitmapB08 = ImageParser.ParseImageStream(imageStreamB08);

                var ndvis = await _processor.ProcessImageToNdviPixels(
                    bitmapB04, 
                    bitmapB08, 
                    polygon, 
                    boundingCoordinates.Item1,
                    boundingCoordinates.Item2);

                return ndvis;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.StackTrace);
            }

            return null;
        }

        private async Task<Stream> GetImageStream((string, Guid) titleAndId, string granuleFolderName, string imageId)
        {
            var response = await _client.GetAsync($"{DataAPI}/odata/v1/Products('{titleAndId.Item2}')" +
                                                  $"/Nodes('{titleAndId.Item1}.SAFE')/Nodes('GRANULE')/Nodes('{granuleFolderName}')/Nodes('IMG_DATA')" +
                                                  $"/Nodes('R10m')/Nodes('{imageId}')/$value");
            response.EnsureSuccessStatusCode();
            var imageStream = await response.Content.ReadAsStreamAsync();
            return imageStream;
        }

        private async Task<string> GetImageId((string, Guid) titleAndId, string granuleFolderName, string imageType)
        {
            string imageId = null;
            var response1 = await _client.GetAsync($"{DataAPI}/odata/v1/Products('{titleAndId.Item2}')" +
                                                  $"/Nodes('{titleAndId.Item1}.SAFE')/Nodes('GRANULE')/Nodes('{granuleFolderName}')/Nodes('IMG_DATA')" +
                                                  $"/Nodes('R10m')/Nodes");
            var xml = new XmlDocument();
            xml.LoadXml(await response1.Content.ReadAsStringAsync());
            var imagesIds = xml.GetElementsByTagName("d:Id");
            foreach (XmlNode imageNode in imagesIds)
            {
                if (imageNode.InnerText.Contains(imageType))
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
                await _client.GetAsync(
                    $"{DataAPI}/odata/v1/Products('{titleAndId.Item2}')/Nodes('{titleAndId.Item1}.SAFE')/Nodes('GRANULE')/Nodes");
            var xml = new XmlDocument();
            xml.LoadXml(await response.Content.ReadAsStringAsync());
            var granuleFolderName = xml.GetElementsByTagName("d:Id")[0]?.InnerText;
            Console.WriteLine($"Granule folder name: {granuleFolderName}");
            return granuleFolderName;
        }
        
        private async Task<(Coordinate, Coordinate)> GetBoundingCoordinates((string, Guid) titleAndId)
        {
            var response =
                await _client.GetAsync(
                    $"{DataAPI}/odata/v1/Products('{titleAndId.Item2}')/Nodes('{titleAndId.Item1}.SAFE')/Nodes('INSPIRE.xml')/$value");
            var xml = new XmlDocument();
            xml.LoadXml(await response.Content.ReadAsStringAsync());
            var boundingBox = xml.GetElementsByTagName("gmd:EX_GeographicBoundingBox")[0];
            var westBound = boundingBox["gmd:westBoundLongitude"]["gco:Decimal"].InnerText;
            var eastBound = boundingBox["gmd:eastBoundLongitude"]["gco:Decimal"].InnerText;
            var southBound = boundingBox["gmd:southBoundLatitude"]["gco:Decimal"].InnerText;
            var northBound = boundingBox["gmd:northBoundLatitude"]["gco:Decimal"].InnerText;

            var topLeft = new Coordinate(southBound, westBound);
            var bottomRight = new Coordinate(northBound, eastBound);
            
            return (topLeft, bottomRight);
        }
        
        private async Task<List<Coordinate>> GetPolygon((string, Guid) titleAndId)
        {
            var response =
                await _client.GetAsync(
                    $"{DataAPI}/odata/v1/Products('{titleAndId.Item2}')/Nodes('{titleAndId.Item1}.SAFE')/Nodes('MTD_MSIL2A.xml')/$value");
            var xml = new XmlDocument();
            xml.LoadXml(await response.Content.ReadAsStringAsync());
            var coordinateString = xml.GetElementsByTagName("EXT_POS_LIST")[0].InnerText;
            var coordinates = coordinateString.Split(" ");

            var polygon = new List<Coordinate>();

            for (int i = 0; i < coordinates.Length - 1; i += 2)
            {
                polygon.Add(new Coordinate(coordinates[i], coordinates[i + 1]));
            }
            
            return polygon;
        }

        private async Task<SearchResult> GetSearchResult()
        {
            this._client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(APIAuth);
            _client.Timeout = new TimeSpan(0, 10, 0);
            var response = await _client.GetAsync(
                $"{DataAPI}/search?q=(footprint:\"Intersects({SearchArea})\" AND platformname:Sentinel-2 AND" +
                $" processinglevel:Level-2A AND platformserialidentifier:Sentinel-2B AND" +
                $" ingestiondate:[NOW-1{SearchInterval} TO NOW])"); // AND cloudcoverpercentage:[0 TO 50]
            var responseString = await response.Content.ReadAsStringAsync();

            return new SearchResult(responseString);
        }
    }
}