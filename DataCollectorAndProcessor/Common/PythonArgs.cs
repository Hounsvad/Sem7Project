using System.Collections.Generic;

namespace Sem7.Input.Common
{
    public class PythonArgs
    {
        public string appsettings;
        public string lat1;
        public string lat2;
        public string long1;
        public string long2;
        public List<JsonCoordinate> polygon;
    }

    public class JsonCoordinate
    {
        public string lat;
        public string @long;
    }
}