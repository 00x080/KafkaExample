using Confluent.Kafka;

namespace InventoryConsumer.Services
{
    public class ConsumerService : BackgroundService
    {
        private readonly IConsumer<Ignore, string> _consumer;

        private readonly ILogger<ConsumerService> _logger;

        public ConsumerService(IConfiguration configuration, ILogger<ConsumerService> logger)
        {
            _logger = logger;

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                GroupId = "InventoryConsumerGroup",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe("InventoryUpdates");

            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessKafkaMessage(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }

            _consumer.Close();
        }

        public async Task ProcessKafkaMessage(CancellationToken stoppingToken)
        {
            try
            {
                var consumeResult = await Task.Run(() => _consumer.Consume(stoppingToken), stoppingToken);

                var message = consumeResult.Message.Value;

                _logger.LogInformation($"Consumer received inventory update: {message}");

                // For greater control and reliability, especially if your consumer needs to guarantee that messages
                // are processed before committing the offset, you can manage offsets manually. This allows you
                // to commit the offset only after successfully processing the message, ensuring that it won't
                // be re-consumed if the consumer crashes or restarts.
                _consumer.Commit(consumeResult);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing Kafka message: {ex.Message}");
            }
        }
    }
}
