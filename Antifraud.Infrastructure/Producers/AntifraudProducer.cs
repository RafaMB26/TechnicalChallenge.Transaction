using Antifraud.Domain.Config;
using Antifraud.Domain.Interfaces.Producer;
using Common.DTOs;
using Common.Result;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Antifraud.Infrastructure.Producers;

public class AntifraudProducer : IAntifraudProducer
{
    private readonly AppSettings _appSettings;
    private readonly ILogger<AntifraudProducer> _logger;
    public AntifraudProducer(ILogger<AntifraudProducer> logger, IOptions<AppSettings> appsettings)
    {
        _logger = logger;
        _appSettings = appsettings.Value;
    }
    public async Task<Result<bool>> ProduceAsync(TransactionProcessedStatusDTO transactionProcessedStatus)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = _appSettings.KafkaServer,
            AllowAutoCreateTopics = true,
            Acks = Acks.Leader
        };

        using var producer = new ProducerBuilder<Null, string>(config).Build();
        try
        {
            var transaction = JsonSerializer.Serialize(transactionProcessedStatus);

            var deliveryResult = await producer.ProduceAsync(
                "topic-transaction-status",
                new Message<Null, string> { Value = transaction });

            _logger.LogInformation("Message delivered: {transactionEvent}", transactionProcessedStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error on {nameof}", nameof(ProduceAsync));
            return new Result<bool>(new Common.Result.Error("Unexpected error on the transaction producer", 503));
        }
        finally
        {
            producer.Flush();
        }
        return new Result<bool>(true);
    }
}
