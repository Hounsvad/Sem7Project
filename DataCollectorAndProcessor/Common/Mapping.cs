using System.Collections.Generic;

namespace Sem7.Input.Common
{
    public class Mapping
    {
        private Mapping(){}

        public static Mapping MappingFactory(Coordinate topLeft, Coordinate bottomRight, int width, int height)
        {
            var returnMapping = new Mapping();
            
            return returnMapping;
        }
        
        private List<Coordinate> ReduceBoundryTo4(List<Coordinate> boundry)
        {
            List<Coordinate> returnList = new List<Coordinate>(boundry.Count);
            boundry.ForEach(coordinate =>
            {
                
            });

            return returnList;
        }
    }
}