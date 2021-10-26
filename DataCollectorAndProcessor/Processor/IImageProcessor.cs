using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sem7.Input.Common;

namespace Sem7.Input.Processor
{
    interface IImageProcessor
    {
        /// <summary>
        /// Returns NDVI values for all pixels, by their individual pixels.
        /// </summary>
        /// <param name="nearInfrared">Bitmap of the near infrared spectrum B04 on Copernicus Sentinel 2 MIR A2</param>
        /// <param name="infrared">Bitmap of the early infrared spectrum. B08 on Copernicus Sentinel 2 MIR A2</param>
        /// <param name="imagePolygon">List of coordinates making up the corners of the image, coordinates listed as Lattitude, Longtitude, with excately 6 digits of accuracy after the comma</param>
        /// <returns><see cref="NDVIPixel"/></returns>
        public Task<NDVIPixel[]> ProcessImageToNdviPixels(Bitmap nearInfrared, Bitmap infrared, List<(int, int)> imagePolygon);
    }
}
