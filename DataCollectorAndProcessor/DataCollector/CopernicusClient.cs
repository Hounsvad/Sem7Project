using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml;
using DataCollector.CopernicusDataStructures;
using DataCollectorAndProcessor;
using Newtonsoft.Json;
using Sem7.Input.Common;
using Sem7.Input.DataCollector;

namespace DataCollector
{
    public class CopernicusClient
    {
        private HttpClient _client = new HttpClient();

        private readonly string DataAPI = Environment.GetEnvironmentVariable("DataAPI");
        private readonly string APIAuth = Environment.GetEnvironmentVariable("APIAuth");
        private readonly string SearchArea = Environment.GetEnvironmentVariable("SearchArea");
        private readonly string SearchInterval = Environment.GetEnvironmentVariable("CopernicusSearchInterval");

        public async Task Execute()
        {
            Console.WriteLine("Connecting to Copernicus");
            var searchResult = await GetSearchResult();
            

            if (!searchResult.FoundResults())
            {
                Console.WriteLine("Did not find any new images");
                return;
            }

            for( int i = 0; i<searchResult.ResultsCount; i++) { 
                try
                {
                    await ExcecuteSingle(searchResult.GetTitleAndIdOfEntry(i));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + e.StackTrace);
                    continue;
                }
                break;
            }
        }

        private async Task ExcecuteSingle((string, Guid) titleAndId )
        {
            var boundingCoordinates = await GetBoundingCoordinates(titleAndId);
            var polygon = await GetPolygon(titleAndId);

            if (!File.Exists(Environment.GetEnvironmentVariable("hdfsImageIngestPath") + "/" +
                             Environment.GetEnvironmentVariable("hdfsImageIngestRedImage")) &&
                !File.Exists(Environment.GetEnvironmentVariable("hdfsImageIngestPath") + "/" +
                             Environment.GetEnvironmentVariable("hdfsImageIngestNirImage")))
            {
                var granuleFolderName = await GetGranuleFolderName(titleAndId);

                var imageB04Id = await GetImageId(titleAndId, granuleFolderName, ImageTypes.B04.ToString());
                var imageB08Id = await GetImageId(titleAndId, granuleFolderName, ImageTypes.B08.ToString());

                Console.WriteLine("Downloading images");
                var imageStreamB04 = await GetImageStream(titleAndId, granuleFolderName, imageB04Id);
                var imageStreamB08 = await GetImageStream(titleAndId, granuleFolderName, imageB08Id);

                Console.WriteLine("Reformatting images in opj_decompress");
                await ImageParser.ParseImageStream(imageStreamB04,
                    Environment.GetEnvironmentVariable("hdfsImageIngestPath") + "/" +
                    Environment.GetEnvironmentVariable("hdfsImageIngestRedImage"));
                await ImageParser.ParseImageStream(imageStreamB08,
                    Environment.GetEnvironmentVariable("hdfsImageIngestPath") + "/" +
                    Environment.GetEnvironmentVariable("hdfsImageIngestNirImage"));
            }

            var args = await CreatePythonArgs(boundingCoordinates, polygon);

            var python = new Process()
            {
                StartInfo =
                    {
                        FileName = "python3",
                        ArgumentList = { Environment.GetEnvironmentVariable("pythonProcessor"), args}
                    }
            };
            python.ErrorDataReceived += (sender, eventArgs) => { Console.WriteLine(eventArgs.Data); };
            python.OutputDataReceived += (sender, eventArgs) => { Console.WriteLine(eventArgs.Data); };
            Console.WriteLine("Processing images with python");
            python.Start();
            await python.WaitForExitAsync();
            Console.WriteLine("Processing done");
        }

        private static async Task<string> CreatePythonArgs((Coordinate, Coordinate) boundingCoordinates, List<Coordinate> polygon)
        {
            var appSettings = ConfigLoader.LoadFromAppConfig();
            var tempPath = Path.GetTempFileName();
            await using (var writer = new StreamWriter(File.OpenWrite(tempPath)))
            {
                var jsonAppSettings = JsonConvert.SerializeObject(appSettings);
                await writer.WriteAsync(jsonAppSettings);
                writer.Close();
            }

            var args = ConfigLoader.CreateArgsForPython(tempPath,
                boundingCoordinates.Item1.Lattitude,
                boundingCoordinates.Item2.Lattitude,
                boundingCoordinates.Item1.Longtitude,
                boundingCoordinates.Item2.Longtitude,
                polygon);
            return args;
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

            var topLeft = new Coordinate(northBound, westBound);
            var bottomRight = new Coordinate(southBound, eastBound);
            
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