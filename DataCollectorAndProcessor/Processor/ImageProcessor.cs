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
        public async Task<NDVIPixel[]> ProcessImageToNdviPixels(Bitmap red, Bitmap nearInfrared, List<Coordinate> imagePolygon, Coordinate topLeft, Coordinate bottomRight)
        {
            var maxLength = red.Height * red.Width;
            var pixels = new List<NDVIPixel>(maxLength);
            int pixelIndex = 0;
            var mapping = Mapping.MappingFactory(topLeft, bottomRight, red.Width, red.Height);
            for (int height = 0; height < red.Height; height++)
            { 
                for (int width = 0; width < red.Width; width++)
                {
                    var redIntensity = red.GetPixel(width, height).R;
                    var nearInfraredIntensity = nearInfrared.GetPixel(width, height).R;
                    var ndvi = (nearInfraredIntensity - redIntensity) /
                               (nearInfraredIntensity + redIntensity);
                    mapping.MapPixel(width, height, out var topLeftCoordinate, out var bottomRightCoordinate);
                    
                    var pixel = new NDVIPixel((sbyte)(ndvi*100), topLeftCoordinate, bottomRightCoordinate);
                    pixels.Add(pixel);
                }
            }
            
            return pixels.ToArray();
        }
    }
}