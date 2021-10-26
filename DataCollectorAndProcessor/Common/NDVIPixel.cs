using System;

namespace Sem7.Input.Common
{
    /// <summary>
    /// NDVI Pixel is a structure representing the ndvi value for an area defined by two points.
    /// The NDVI value is stored in an sbyte, which stores the -1.00 to 1.00 as -100 to 100 to avoid float errors
    /// </summary>
    public struct NDVIPixel
    {
        public sbyte NdviValue;
        public Coordinate TopLeft;
        public Coordinate BottomRight;

        public NDVIPixel(sbyte ndviValue, Coordinate topLeft, Coordinate bottomRight)
        {
            NdviValue = ndviValue;
            TopLeft = topLeft;
            BottomRight = bottomRight;
        }
    }
}
