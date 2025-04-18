using Confluent.Kafka;

namespace InventoryConsumer.Services
{
    public class ConsumerService : BackgroundService
    {
        private readonly IConsumer<Ignore, string> _consumer;

        private readonly ILogger<ConsumerService> _logger;
        private readonly ConsumerConfig _consumerConfig;

        public ConsumerService(IConfiguration configuration, ILogger<ConsumerService> logger)
        {
            _logger = logger;

            var bootstrapServers = configuration["Kafka:BootstrapServers"];

            _logger.LogInformation($"Configuring Kafka to Bootstrap Servers {bootstrapServers}");

            _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = "InventoryConsumerGroup",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            _consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async() =>
            {
                try
                {
                    SubscribeToKafka();

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        await CheckForKafkaMessage(stoppingToken);
                        Thread.Sleep(1000); // Sleep for a short duration to avoid busy waiting
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Kafka polling was cancelled.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception in ExecuteAsync");
                }
                finally
                {
                    _logger.LogInformation("ConsumerService is stopping and Kafka consumer is closed.");
                    _consumer.Close();
                }
            }, stoppingToken);
        }

        private void SubscribeToKafka()
        {
            _logger.LogInformation("Consumer is subscribing to InventoryUpdates topic.");

            var subscribed = false;

            while (!subscribed)
            {
                try
                {
                    _consumer.Subscribe("InventoryUpdates");
                    subscribed = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error subscribing to Kafka topic.");
                    Thread.Sleep(1000); // Retry after a short delay
                }
            }

            _logger.LogInformation("Consumer is successfully subscribed to InventoryUpdates topic.");
        }

        public async Task CheckForKafkaMessage(CancellationToken stoppingToken)
        {
            await Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("Polling Kafka for a message...");

                    var consumeResult = _consumer.Consume(stoppingToken);

                    if (consumeResult != null)
                    {
                        _logger.LogInformation($"Received message: {consumeResult.Message.Value}");
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Kafka polling was cancelled.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing Kafka message.");
                }
            }, stoppingToken);
        }
    }
}
