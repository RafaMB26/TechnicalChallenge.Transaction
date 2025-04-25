using Antifraud.Domain.Config;
using Antifraud.Domain.Interfaces.Services;
using Common.DTOs;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Antifraud.Presentation.Consumers
{
    public class TransactionEventConsumer: BackgroundService
    {
        private readonly ILogger<TransactionEventConsumer> _logger;
        private readonly AppSettings _appSettings;
        private readonly IServiceProvider _serviceProvider;
        public TransactionEventConsumer(
            IServiceProvider serviceProvider, 
            IOptions<AppSettings> appSettings, 
            ILogger<TransactionEventConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _appSettings = appSettings.Value;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _appSettings.KafkaServer,
                GroupId = "group-transaction",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
                    consumer.Subscribe("topic-transaction");
                    using var scope = _serviceProvider.CreateScope();
                    IAntifraudService _antifraudService = scope.ServiceProvider.GetRequiredService<IAntifraudService>();
                    try
                    {
                        var consumeResult = consumer.Consume(TimeSpan.FromSeconds(5));
                        if (consumeResult == null)
                            continue;

                        var transactionEvent = consumeResult.Message.Value;
                        if (transactionEvent is null)
                            continue;

                        var transaction = JsonSerializer.Deserialize<TransactionDTO>(transactionEvent);
                        if (transaction is null)
                            continue;

                        var isTransactionCorrectResult = await _antifraudService.IsTransactionCorrectAsync(transaction);
                        if (!isTransactionCorrectResult.IsSuccess)
                            _logger.LogError("An unexpected error happened while trying to validate the transaction. Error {isTransactionCorrectResult}", isTransactionCorrectResult.Error);

                    }
                    catch (KafkaException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                    {
                        _logger.LogWarning("The topic is not available yet. Retrying in 10 seconds...");
                        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An unexpected error happened at {nameof}", nameof(ExecuteAsync));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error on {nameof}", nameof(ExecuteAsync));
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
        }
    }
}
