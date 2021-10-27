using System.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Confluent.Kafka;
using Sem7.Input.Common;

namespace Sem7.Input.DataCollector
{
    public class KafkaClient
    {
        public readonly ProducerConfig _ProducerConfig;

        public KafkaClient()
        {
            _ProducerConfig = new ProducerConfig()
            {
                BootstrapServers = ConfigurationManager.AppSettings.Get("KafkaAddress")
            };
        }
        
        public async Task Produce(NDVIPixel[] ndvis)
        {
            using (var producer = new ProducerBuilder<Null, string>(_ProducerConfig).Build())
            {
                foreach (var ndvi in ndvis)
                {
                    await producer.ProduceAsync(ConfigurationManager.AppSettings.Get("KafkaTopic"),
                        new Message<Null, string>()
                        {
                            Value = JsonSerializer.Serialize(ndvi)
                        });
                }
            }
        }
    }
}