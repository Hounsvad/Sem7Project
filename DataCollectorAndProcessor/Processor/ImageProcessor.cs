using System.Collections.Generic;
using System.Drawing;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using Sem7.Input.Common;

namespace Sem7.Input.Processor
{
    public class ImageProcessor : IImageProcessor
    {
        
        /// <inheritdoc cref="IImageProcessor.ProcessImageToNdviPixels"/>
        public async Task<NDVIPixel[]> ProcessImageToNdviPixels(Bitmap nearInfrared, Bitmap infrared, List<Coordinate> imagePolygon, Coordinate topLeft, Coordinate bottomRight)
        {
            var maxLength = nearInfrared.Height * nearInfrared.Width;
            var pixels = new NDVIPixel[maxLength];

            //var mapping = Mapping.( imagePolygon);
            
            for (int height = 0; height < nearInfrared.Height; height++)
            { 
                for (int width = 0; width < nearInfrared.Width; width++)
                {
                    
                }
            }
            
            return pixels;
        }
    }
}