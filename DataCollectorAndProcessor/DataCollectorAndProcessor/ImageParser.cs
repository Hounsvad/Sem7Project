using FreeImageAPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCollectorAndProcessor
{
    class ImageParser
    {
        public async static Task ParseImage(String file)
        {
            var fileTokens = file.Split('/');

            string imageName = fileTokens.Last();
            string imagePath = file.Remove(file.Length - imageName.Length);
            string imageBMPOut = $"{imagePath}{imageName.Split('.')[0]}.bmp";
            string imageCSVOut = $"{imagePath}{imageName.Split('.')[0]}.csv";
            var image = FreeImage.Load(FREE_IMAGE_FORMAT.FIF_JP2, $"{imagePath}{imageName}", FREE_IMAGE_LOAD_FLAGS.DEFAULT);
            FreeImage.Save(FREE_IMAGE_FORMAT.FIF_BMP, image, imageBMPOut, FREE_IMAGE_SAVE_FLAGS.DEFAULT);
            var betterImage = new Bitmap(imageBMPOut);

            using (FileStream outstream = File.OpenWrite(imageCSVOut))
            {
                outstream.Write(Encoding.UTF8.GetBytes("X,Y,HEXCOLOR"));
                List<Task> tasks = new();
                for (int x = 0; x < betterImage.Width; x++)
                {
                    for (int y = 0; y < betterImage.Height; y++)
                    {
                        tasks.Add(Task.Run(() => WriteResultString(betterImage.GetPixel(x, y), x, y, outstream)));
                    }
                }
                await Task.WhenAll(tasks);
                outstream.Close();
            }
        }
        public static async Task WriteResultString(Color pixelValue, int x, int y, FileStream stream)
        {
            await stream.WriteAsync(Encoding.UTF8.GetBytes($"{x},{y},{pixelValue.R:X2}{pixelValue.G:X2}{pixelValue.B:X2}"));
        }
    }
}
