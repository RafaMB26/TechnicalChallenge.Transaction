using Common.DTOs;
using Common.Enums;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Transaction.Domain.Config;
using Transaction.Domain.Interfaces.Services;

namespace Transaction.Ports.KafkaConsumers.Consumers;

public class TransactionStatusEventConsumer : BackgroundService
{
    private readonly AppSettings _appSettings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TransactionStatusEventConsumer> _logger;
    private readonly IConsumer<Null, string> _consumer;
    public TransactionStatusEventConsumer(
        IServiceProvider serviceProvider,
        IOptions<AppSettings> appSettings,
        ILogger<TransactionStatusEventConsumer> logger)
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
            _consumer.Subscribe("topic-transaction-status");
            using var scope = _serviceProvider.CreateScope();
            ITransactionService _transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(5));
                    if (consumeResult == null)
                        continue;

                    var transactionEvent = consumeResult.Message.Value;
                    if (transactionEvent is null)
                        continue;

                    var transaction = JsonSerializer.Deserialize<TransactionProcessedStatusDTO>(transactionEvent);
                    if (transaction is null)
                        continue;

                    var updateResult = await _transactionService.UpdateTransactionStatus(transaction);
                    if (!updateResult.IsSuccess)
                        _logger.LogError("An unexpected error happened while trying to update the transaction {transactionExternalId}: Error {message}", transaction.TransactionExternalId, updateResult.Error.Message);
                    else
                    {
                        var transactionStatus = transaction.IsCorrect ? TransactionStatusEnum.Approved : TransactionStatusEnum.Approved;
                        _logger.LogInformation("Transaction updated {transactionExternalId} to {transactionStatus}", transaction.TransactionExternalId, transactionStatus.ToString());
                    }

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
        });
    }
}
