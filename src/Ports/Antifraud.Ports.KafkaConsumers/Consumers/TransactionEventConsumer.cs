using Antifraud.Domain.Config;
using Antifraud.Domain.Interfaces.Services;
using Common.DTOs;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Antifraud.Ports.KafkaConsumers.Consumers
{
    public class TransactionEventConsumer: BackgroundService
    {
        private readonly ILogger<TransactionEventConsumer> _logger;
        private readonly AppSettings _appSettings;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConsumer<Null, string> _consumer;
        public TransactionEventConsumer(
            IServiceProvider serviceProvider, 
            IOptions<AppSettings> appSettings, 
            ILogger<TransactionEventConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _appSettings = appSettings.Value;
            var config = new ConsumerConfig
            {
                BootstrapServers = _appSettings.KafkaServer,
                GroupId = "group-transaction",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _consumer = new ConsumerBuilder<Null, string>(config).Build();
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () => 
            {
                _consumer.Subscribe("topic-transaction");
                using var scope = _serviceProvider.CreateScope();
                IAntifraudService antifraudService = scope.ServiceProvider.GetRequiredService<IAntifraudService>();
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(5));
                        if (consumeResult == null || consumeResult.Message?.Value == null)
                            continue;

                        var transaction = JsonSerializer.Deserialize<TransactionDTO>(consumeResult.Message.Value);
                        if (transaction == null)
                            continue;

                        var result = await antifraudService.IsTransactionCorrectAsync(transaction);
                        if (!result.IsSuccess)
                        {
                            _logger.LogError("Error validating transaction: {error}", result.Error);
                        }
                    }
                    catch (KafkaException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                    {
                        _logger.LogWarning("The topic is not available yet. Retrying in 10 seconds...");
                        //await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error in Kafka consumer.");
                    }
                }
            });
        }
    }
}
