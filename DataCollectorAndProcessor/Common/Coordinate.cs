using System;
using System.Reflection.PortableExecutable;

namespace Sem7.Input.Common
{
    public class Coordinate
    {
        public int Lattitude { get; set; }
        public int Longtitude { get; set; }

        /// <summary>
        /// Instantiates a coordinate
        /// The coordinates are stored with fixed point precision of 6 decimal digits (1/1000000)
        /// </summary>
        public Coordinate(int lattitude, int longtitude)
        {
            Lattitude = lattitude;
            Longtitude = longtitude;
        }

        /// <summary>
        /// Instantiates a coordinate
        /// The coordinates are stored with fixed point precision of 6 decimal digits (1/1000000)
        /// The input doubles are converted by multiplying them by 10^6 and then rounding down, before storing in an int
        /// </summary>
        /// <param name="lattitude"></param>
        /// <param name="longtitude"></param>
        public Coordinate(double lattitude, double longtitude)
        {
            decimal _lattitude = (decimal) lattitude;
            decimal _longtitude = (decimal) longtitude;
            Lattitude = (int)Math.Floor(_lattitude * 1000000m);
            Longtitude = (int)Math.Floor(_longtitude * 1000000m);
        }

        public Coordinate(string lattitude, string longtitude)
        {
            Lattitude = int.Parse(lattitude.Split('.')[0] + lattitude.Split('.')[1].Substring(0,6));
            Longtitude = int.Parse(longtitude.Split('.')[0] + longtitude.Split('.')[1].Substring(0, 6));

        }
    }
}