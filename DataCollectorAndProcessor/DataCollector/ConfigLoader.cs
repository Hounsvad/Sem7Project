using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Newtonsoft.Json;
using Sem7.Input.Common;

namespace Sem7.Input.DataCollector
{
    public class ConfigLoader
    {
        public static AppSettings LoadFromAppConfig() => new AppSettings
        {
            hdfsImageIngestPath = Environment.GetEnvironmentVariable("hdfsImageIngestPath"),
            hdfsImageIngestRedImage = Environment.GetEnvironmentVariable("hdfsImageIngestRedImage"),
            hdfsImageIngestNirImage = Environment.GetEnvironmentVariable("hdfsImageIngestNirImage"),
            kafkaURL = Environment.GetEnvironmentVariable("kafkaURL"),
            ndviPixelIngestTopic = Environment.GetEnvironmentVariable("ndviPixelIngestTopic"),
            sparkAppName = Environment.GetEnvironmentVariable("sparkAppName"),
            sparkMasterURL = Environment.GetEnvironmentVariable("sparkMasterURL"),
            sparkMaxMemory = Environment.GetEnvironmentVariable("sparkMaxMemory"),
            sparkCheckpointLocationURL = Environment.GetEnvironmentVariable("sparkCheckpointLocationURL")
        };


        public static string CreateArgsForPython(string appSettings, int lat1, int lat2, int long1, int long2,
            List<Coordinate> polygon) => JsonConvert.SerializeObject(new PythonArgs()
        {
            appsettings = appSettings,
            lat1 = lat1.ToString(),
            lat2 = lat2.ToString(),
            long1 = long1.ToString(),
            long2 = long2.ToString(),
            polygon = polygon.Select(x => x.ToJson()).ToList()
        });
    }
}