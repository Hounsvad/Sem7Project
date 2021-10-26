using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Sem7.Input.Common;

namespace Sem7.Input.Processor
{
    public class ImageProcessor : IImageProcessor
    {
        
        /// <inheritdoc cref="IImageProcessor.ProcessImageToNdviPixels"/>
        public async Task<NDVIPixel[]> ProcessImageToNdviPixels(Bitmap nearInfrared, Bitmap infrared, List<Coordinate> imagePolygon)
        {
            var maxLength = nearInfrared.Height * nearInfrared.Width;
            var pixels = new NDVIPixel[maxLength];

            for (int height = 0; height < nearInfrared.Height; height++)wt
            {
                for (int width = 0; width < nearInfrared.Width; width++)
                {
                    
                }
            }
            
            return pixels;
        }

        private Mapping CalculateCoordinateMapping()
        {
            Coordinate LeftUpper;
            Coordinate RightUpper;
            Coordinate RightLower;
            Coordinate LeftLower;
            
            
        }

        private (Coordinate, Coordinate) CalculateCoordinates(image)
        {
            
        }
    }
}