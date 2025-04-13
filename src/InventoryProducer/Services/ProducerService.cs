using Confluent.Kafka;

namespace InventoryProducer.Services;

public class ProducerService
{
    private readonly IConfiguration _configuration;

    private readonly IProducer<Null, string> _producer;

    public ProducerService(IConfiguration configuration)
    {
        _configuration = configuration;

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"]
        };

        // create an instance of lidrd kafka
        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
    }

    public async Task ProduceAsync(string topic, string message)
    {
        var kafkaMessage = new Message<Null, string> { Value = message, };

        await _producer.ProduceAsync(topic, kafkaMessage);
    }
}

