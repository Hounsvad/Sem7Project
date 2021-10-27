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
            var pixels = new List<NDVIPixel>(maxLength);
            int pixelIndex = 0;
            var mapping = Mapping.MappingFactory(topLeft, bottomRight, nearInfrared.Width, nearInfrared.Height);
            for (int height = 0; height < nearInfrared.Height; height++)
            { 
                for (int width = 0; width < nearInfrared.Width; width++)
                {
                    var nearInfraredIntensity = nearInfrared.GetPixel(width, height).R;
                    var infraredIntensity = infrared.GetPixel(width, height).R;
                    var ndvi = (infraredIntensity - nearInfraredIntensity) /
                               (infraredIntensity + nearInfraredIntensity);
                    mapping.MapPixel(width, height, out var topLeftCoordinate, out var bottomRightCoordinate);
                    
                    var pixel = new NDVIPixel((sbyte)(ndvi*100), topLeftCoordinate, bottomRightCoordinate);
                    pixels.Add(pixel);
                }
            }
            
            return pixels.ToArray();
        }
    }
}