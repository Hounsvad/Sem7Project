using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Processor
{
    interface IImageProcessor
    {
        /// <summary>
        /// Returns NDVI values for all pixels, by their individual pixels.
        /// </summary>
        /// <param name="nearInfrared"></param>
        /// <param name="infrared"></param>
        /// <param name="imagePolygon"></param>
        /// <returns><see cref="NDVIPixel"/></returns>
        public NDVIPixel[] ProcessImageToNdviPixels(Bitmap nearInfrared, Bitmap infrared, List<(int, int)> imagePolygon);
    }
}
