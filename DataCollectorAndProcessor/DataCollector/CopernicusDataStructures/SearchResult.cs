using System;
using System.Xml;

namespace DataCollector.CopernicusDataStructures
{
    public class SearchResult
    {
        private XmlDocument Xml;
        private readonly int ResultsCount;
        
        public SearchResult(string xmlAsString)
        {
            Xml = new XmlDocument();
            Xml.LoadXml(xmlAsString);
            var results = Xml.GetElementsByTagName("opensearch:totalResults")[0]?.InnerText;
            Console.WriteLine($"Found {results ?? "no"} search results");
            if (String.IsNullOrWhiteSpace(results) || !int.TryParse(results, out var intResults) || intResults < 1)
            {
                ResultsCount = 0;
            }
            else
            {
                ResultsCount = intResults;
            }
        }

        public bool FoundResults()
        {
            return ResultsCount > 0;
        }

        public (string, Guid) GetTitleAndIdOfFirstEntry()
        {
            var title = Xml.GetElementsByTagName("title")[1]?.InnerText;
            var id = Xml.GetElementsByTagName("id")[1]?.InnerText;
            if (id == null || title == null)
                throw new ArgumentNullException($"id is null?{id == null}: title is null?{title == null}");
            return (title, Guid.Parse(id));
        }
    }
}