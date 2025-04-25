using Common.DTOs;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Transaction.Domain.Config;
using Transaction.Domain.Interfaces.Services;

namespace Transaction.Presentation.Consumers
{
    public class TransactionStatusEventConsumer : BackgroundService
    {
        private readonly AppSettings _appSettings;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TransactionStatusEventConsumer> _logger;
        public TransactionStatusEventConsumer(
            IServiceProvider serviceProvider,
            IOptions<AppSettings> appSettings,
            ILogger<TransactionStatusEventConsumer> logger)
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
                    consumer.Subscribe("topic-transaction-status");
                    using var scope = _serviceProvider.CreateScope();
                    ITransactionService _transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

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

                        var updateResult = await _transactionService.UpdateTransactionStatus(transaction);
                        if (!updateResult.IsSuccess)
                            _logger.LogError($"An unexpected error happened while trying to update the transaction {transaction.TransactionExternalId}: Error {updateResult.Error.Message}");
                        else
                            _logger.LogInformation($"Transaction updated {transaction.TransactionExternalId} to {transaction.Status}");
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
                    _logger.LogError(ex,"Unexpected error on {nameof}", nameof(ExecuteAsync));
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
        }
    }
}