using Confluent.Kafka;

namespace MTOGO.API.Kafka
{
    // Extend BackgroundService to run in the background for the lifetime of the application
    public class KafkaConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<KafkaConsumer> _logger;

        // The Confluent.Kafka IConsumer interface with Key-Value, like IProducer
        private readonly IConsumer<string, string> _consumer;

        public KafkaConsumer(IConfiguration configuration, ILogger<KafkaConsumer> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var config = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],

                //These configs should probably also be setup in application.json:
                // The consumer group ID; consumers with the same group ID share load and process messages as a group. 
                GroupId = "groupId",
                // Configures the consumer to read from the beginning of the topic if there’s no offset.
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            // We build the consumer instance with the specified configs.
            _consumer = new ConsumerBuilder<string, string>(config).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Subscribe the consumer to the specified topic so it can receive messages from it. Should also be configured somewhere
            _consumer.Subscribe("topic");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Kafka consumer is running.");
                    try
                    {
                        var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(5)); // Is here to not block swagger 

                        if (consumeResult != null)
                        {
                            // Process the message if one was received
                            var message = consumeResult.Message.Value;
                            var key = consumeResult.Message.Key;

                            // Here we just log it, but should call other services to process the message instead
                            _logger.LogInformation($"Received Message: {message}, Key: {key}");
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError($"Error consuming Kafka message: {ex.Message}");
                    }
                    // Adding a delay which is non-blocking and will stop, if the application is shutting down
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Kafka consumer stopping gracefully.");
            }
            finally
            {
                _consumer.Close();
                _logger.LogInformation("Kafka consumer has stopped.");
            }
        }
    }
}
