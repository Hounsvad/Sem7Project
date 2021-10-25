using System;

namespace Processor
{
    /// <summary>
    /// NDVI Pixel is a structure representing the ndvi value for an area defined by two points.
    /// The NDVI value is stored in an sbyte, which stores the -1.00 to 1.00 as -100 to 100 to avoid float errors
    /// </summary>
    public struct NDVIPixel
    {
        public sbyte NdviValue;
        public int Lattitude1;
        public int Lattitude2;
        public int Longtitude1;
        public int Longtitude2;

        public NDVIPixel(sbyte ndviValue, int lattitude1, int lattitude2, int longtitude1, int longtitude2)
        {
            NdviValue = ndviValue;
            Lattitude1 = lattitude1;
            Lattitude2 = lattitude2;
            Longtitude1 = longtitude1;
            Longtitude2 = longtitude2;
        }
    }
}
