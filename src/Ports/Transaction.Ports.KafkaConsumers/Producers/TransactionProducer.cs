using Common.DTOs;
using Common.Result;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Transaction.Domain.Config;
using Transaction.Domain.Interfaces.Producers;

namespace Transaction.Ports.KafkaConsumers.Producers
{
    public class TransactionProducer : ITransactionProducer
    {
        private readonly ILogger<TransactionProducer> _logger;
        private readonly AppSettings _appSettings;
        public TransactionProducer(IOptions<AppSettings> appSettings, ILogger<TransactionProducer> logger)
        {
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        public async Task<Result<bool>> ProduceAsync(TransactionDTO transactionEvent)
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
                var transactionSerialized = JsonSerializer.Serialize(transactionEvent);
                var deliveryResult = await producer.ProduceAsync(
                    "topic-transaction",
                    new Message<Null, string> { Value = transactionSerialized });

                _logger.LogInformation("Message delivered: {transactionEvent}", transactionEvent);
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

}
