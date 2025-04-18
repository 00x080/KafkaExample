using Confluent.Kafka;

namespace InventoryProducer.Services;

public class ProducerService
{
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<ProducerService> _logger;

    public ProducerService(IConfiguration configuration, ILogger<ProducerService> logger)
    {
        _logger = logger;
        _configuration = configuration;

        var bootstrapServers = _configuration["Kafka:BootstrapServers"];

        _logger.LogInformation($"Configuring Kafka to Bootstrap Servers {bootstrapServers}");

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };

        // create an instance of lidrd kafka
        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
    }

    public async Task ProduceAsync(string topic, string message)
    {
        try
        {
            var kafkaMessage = new Message<Null, string> { Value = message, };

            var result = await _producer.ProduceAsync(topic, kafkaMessage);
            _logger.LogInformation($"Message sent to topic {result.Topic}, partition {result.Partition}, offset {result.Offset}");
            //await _producer.ProduceAsync(topic, kafkaMessage);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error producing message to Kafka.");
        }
    }
}

