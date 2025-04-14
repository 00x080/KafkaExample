using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace InventoryConsumer.Services
{
    public class ConsumerService : BackgroundService
    {
        private readonly IConsumer<Ignore, string> _consumer;

        private readonly ILogger<ConsumerService> _logger;

        public ConsumerService(IConfiguration configuration, ILogger<ConsumerService> logger)
        {
            _logger = logger;

            _logger.LogInformation("ConsumerService started and polling for messages.");

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                GroupId = "InventoryConsumerGroup",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                SessionTimeoutMs = 6000,
            };

            _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();

            _logger.LogInformation("Consumer is subscribing to InventoryUpdates topic.");
            _consumer.Subscribe("InventoryUpdates");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ConsumerService started and polling for messages.");

            _consumer.Subscribe("InventoryUpdates");
            _logger.LogInformation("Consumer is subscribing to InventoryUpdates topic.");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("ConsumerService is polling for Kafka messages.");
                    await ProcessKafkaMessage(stoppingToken);

                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    }
                    catch (TaskCanceledException)
                    {
                        // Token was cancelled during delay — exit gracefully
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("ConsumerService cancellation requested.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in ExecuteAsync");
            }
            finally
            {
                _consumer.Close();
                _logger.LogInformation("ConsumerService is stopping and Kafka consumer is closed.");
            }
        }

        public async Task ProcessKafkaMessage(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation($"Cancellation requested before consuming: {stoppingToken.IsCancellationRequested}");

                _logger.LogInformation("Polling Kafka for a message...");

                var consumeResult = await Task.Run(() => _consumer.Consume(stoppingToken), stoppingToken);

                if (consumeResult != null)
                {
                    _logger.LogInformation("Kafka message received successfully");

                    var message = consumeResult.Message?.Value ?? "(null)";
                    _logger.LogInformation($"Consumer received inventory update: {message}");
                    _consumer.Commit(consumeResult);
                }
                else
                {
                    _logger.LogWarning("ConsumeResult was null.");
                }
            }
            catch (OperationCanceledException oce)
            {
                _logger.LogWarning(oce, "Kafka polling was cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Kafka message.");
            }
        }
    }
}
