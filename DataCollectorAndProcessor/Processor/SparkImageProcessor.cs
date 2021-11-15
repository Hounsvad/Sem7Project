using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Configuration;
using System.Linq;
using Microsoft.Hadoop.MapReduce;
using Microsoft.Spark;
using Microsoft.Spark.Sql;
using Microsoft.Spark.Sql.Types;
using Sem7.Input.Common;
using static Microsoft.Spark.Sql.Functions;

namespace Sem7.Input.Processor
{
    /// <inheritdoc cref="IImageProcessor.ProcessImageToNdviPixels"/>
    public class SparkImageProcessor : IImageProcessor
    {
        public Task<NDVIPixel[]> ProcessImageToNdviPixels(Bitmap red, Bitmap nearInfrared,
            List<Coordinate> imagePolygon, Coordinate topLeft, Coordinate bottomRight)
        {
            string hdfsImageIngestPath = ConfigurationManager.AppSettings
                .Get("ImageIngestPath") ?? "/defaults/ingest/images/";

            var redImage = SaveImage(red);
            var nirImage = SaveImage(nearInfrared);

            CopyFileToHDFS(redImage, hdfsImageIngestPath + redImage.Split('/').ToList().Last());
            CopyFileToHDFS(nirImage, hdfsImageIngestPath + nirImage.Split('/').ToList().Last());

            var spark = SparkSession.Builder().AppName("WordCountSample").GetOrCreate();

            var redImageDF = spark.Read().Format("image").Load(hdfsImageIngestPath + redImage.Split('/'));
            var nirImageDF = spark.Read().Format("image").Load(hdfsImageIngestPath + nirImage.Split('/'));

            var mapping = Mapping.MappingFactory(topLeft, bottomRight, red.Width, red.Height);

            RegisterUdfs(spark, mapping);

            DataFrame df = redImageDF.Select();

            redImageDF.SelectExpr("select convertToPixels(image.height, image.width, image.data  )");
            return null;
        }

        private void RegisterUdfs(SparkSession spark, Mapping mapping)
        {
            spark.Udf().Register<byte, byte, sbyte>("calculateNDVI", CalculateNDVIValue);
            spark.Udf().Register<int, int, byte[], (int, int, byte)[]>("convertToPixels", ConvertImageDataToPixels);
            spark.Udf().Register<(int, int, sbyte), (int, int, int, int, sbyte)>("mapPixelToCoordinates", ndviPixel =>
            {
                mapping.MapPixel(ndviPixel.Item1, ndviPixel.Item2,
                    out Coordinate topLeftCoordinate, out Coordinate bottomRightCoordinate);
                return (topLeftCoordinate.Lattitude, topLeftCoordinate.Longtitude, bottomRightCoordinate.Lattitude,
                    bottomRightCoordinate.Longtitude, ndviPixel.Item3);
            });
        }

        
        private (int, int, byte)[] ConvertImageDataToPixels(int height, int width, byte[] data)
        {
            var pixels = new (int, int, byte)[data.Length / 3];
            for (int i = 0; i < data.Length; i += 3)
            {
                pixels[i / 3] = (i / width, i % width, data[i]);
            }

            return pixels;
        }

        private sbyte CalculateNDVIValue(byte redPixel, byte nirPixel)
        {
            return (sbyte) ((nirPixel - redPixel) / (nirPixel + redPixel) * 100);
        }

        private void CopyFileToHDFS(string localFile, string remotePath)
        {
            var connection = Hadoop.Connect();
            if (!connection.StorageSystem.Exists(remotePath))
            {
                connection.StorageSystem.MakeDirectory(remotePath);
            }

            connection.StorageSystem.CopyFromLocal(localFile,
                remotePath);
        }

        private string SaveImage(Bitmap image)
        {
            string imageName = Path.GetTempFileName();
            using var filestream = File.OpenWrite(imageName);
            image.Save(imageName, ImageFormat.Bmp);
            return imageName;
        }
    }
}