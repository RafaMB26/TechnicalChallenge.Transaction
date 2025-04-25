using Antifraud.Domain.Config;
using Antifraud.Domain.Interfaces.Services;
using Common.DTOs;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace Antifraud.Presentation.Consumers
{
    public class TransactionEventConsumer: BackgroundService
    {
        private readonly ILogger<TransactionEventConsumer> _logger;
        private readonly AppSettings _appSettings;
        private readonly IAntifraudService _antifraudService;
        public TransactionEventConsumer(
            IAntifraudService antifraudService, 
            IOptions<AppSettings> appSettings, 
            ILogger<TransactionEventConsumer> logger)
        {
            _antifraudService = antifraudService;
            _logger = logger;
            _appSettings = appSettings.Value;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _appSettings.KafkaServer,
                GroupId = "topic-transaction",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, TransactionDTO>(config).Build();
            consumer.Subscribe("group-transaction");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(TimeSpan.FromSeconds(5));
                    if (consumeResult == null)
                        continue;

                    var transactionEvent = consumeResult.Message.Value;
                    if (transactionEvent is null)
                        continue;

                    var isTransactionCorrectResult = await _antifraudService.IsTransactionCorrectAsync(transactionEvent);
                    if (!isTransactionCorrectResult.IsSuccess)
                        _logger.LogError("An unexpected error happened while trying to validate the transaction. Error {isTransactionCorrectResult}", isTransactionCorrectResult.Error);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error happened at {nameof}", nameof(ExecuteAsync));
                }
            }
        }
    }
}
