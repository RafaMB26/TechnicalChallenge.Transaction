
using Common.DTOs;
using Common.Enums;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Transaction.Domain.Config;
using Transaction.Domain.Interfaces.Repositories;

namespace Transaction.Presentation.Consumers
{
    public class TransactionStatusEventConsumer : BackgroundService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITransactionStatusRepository _transactionStatusRepository;
        private readonly AppSettings _appSettings;
        private readonly ILogger<TransactionStatusEventConsumer> _logger;
        public TransactionStatusEventConsumer(
            ITransactionRepository transactionRepository,
            ITransactionStatusRepository transactionStatusRepository,
            IOptions<AppSettings> appSettings,
            ILogger<TransactionStatusEventConsumer> logger)
        {
            _transactionRepository = transactionRepository;
            _transactionStatusRepository = transactionStatusRepository;
            _logger = logger;
            _appSettings = appSettings.Value;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _appSettings.KafkaServer,
                GroupId = "topic-transaction-status",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, TransactionProcessedStatusDTO>(config).Build();
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

                    var currentTransactionResult = await _transactionRepository.GetTransactionByPublicIdAsync(transactionEvent.TransactionExternalId);
                    if (currentTransactionResult.IsSuccess && currentTransactionResult.Data is not null)
                    {
                        var statusToUpdate = transactionEvent.IsCorrect ? TransactionStatusEnum.Approved : TransactionStatusEnum.Rejected;
                        var currentTransaction = currentTransactionResult.Data!;
                        var statusResult = await _transactionStatusRepository.GetTransactionTypeByName(statusToUpdate);
                        
                        if (!statusResult.IsSuccess)
                            _logger.LogError("An error happened while trying to get the status Id {statusToUpdate}. Error : {error}", statusToUpdate, statusResult.Error);

                        currentTransaction.Status = statusResult.IsSuccess ? statusResult.Data : currentTransaction.Status;
                        var statusUpdateResult = await _transactionRepository.UpdateAsync(currentTransaction.Id, currentTransaction);
                        if (!statusUpdateResult.IsSuccess)
                            _logger.LogError("An error happened while trying to update the status of the transaction {transactionExternalId} to {statusToUpdate}", transactionEvent.TransactionExternalId, statusToUpdate);
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error happened at {nameof}", nameof(ExecuteAsync));
                }
            }
        }
    }
}
