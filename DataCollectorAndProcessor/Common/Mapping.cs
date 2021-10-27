using System.Collections.Generic;

namespace Sem7.Input.Common
{
    public class Mapping
    {
        private Coordinate TopLeft;

        private double PixelToCoordinateXFactor;
        private double PixelToCoordinateYFactor;
        
        public static Mapping MappingFactory(Coordinate topLeft, Coordinate bottomRight, int width, int height)
        {
            var returnMapping = new Mapping();
            returnMapping.TopLeft = topLeft;

            returnMapping.PixelToCoordinateYFactor = ((double) (bottomRight.Lattitude - topLeft.Lattitude)) / (height);
            returnMapping.PixelToCoordinateXFactor = ((double) (bottomRight.Longtitude - topLeft.Longtitude)) / (width);
                        
            return returnMapping;
        }

        public void MapPixel(int x, int y, out Coordinate TopLeftCoordinate, out Coordinate BottomRightCoordinate)
        {
            TopLeftCoordinate = new Coordinate(
                TopLeft.Lattitude + PixelToCoordinateYFactor * y,
                TopLeft.Longtitude + PixelToCoordinateXFactor * x);
            BottomRightCoordinate = new Coordinate(
                TopLeft.Lattitude + PixelToCoordinateYFactor * (y + 1),
                TopLeft.Longtitude + PixelToCoordinateXFactor * (x + 1));
        }
    }
}