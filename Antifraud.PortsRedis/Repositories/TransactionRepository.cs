using Antifraud.Domain.Interfaces.Repositories;
using Common.Result;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Antifraud.Ports.Redis.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly IDatabase _db;
    private const string ACCOUNT_TODAY_BALANCE = "{0}:{1}";
    private readonly ILogger<TransactionRepository> _logger;
    public TransactionRepository(IConfiguration configuration, ILogger<TransactionRepository> logger)
    {
        _logger = logger;
        var redis = ConnectionMultiplexer.Connect(configuration.GetConnectionString("RedisServer"));
        _db = redis.GetDatabase();
    }

    public async Task<Result<decimal>> GetSumOfTransactionsAsync(Guid source)
    {
        try
        {
            var today = DateTime.Today.ToString("yyyyMMdd");
            var key = string.Format(ACCOUNT_TODAY_BALANCE, today, source);
            var sum = await _db.StringGetAsync(key);

            return new Result<decimal>(Convert.ToDecimal(sum));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error on {nameof} on account {source}", nameof(GetSumOfTransactionsAsync), source);
            return new Result<decimal>(new Error("Error while trying to get todays balance", 503));
        }
    }

    public async Task<Result<bool>> AddTransactionValueAsync(Guid source, decimal amount)
    {
        try
        {
            var currentResult = await GetSumOfTransactionsAsync(source);
            if (!currentResult.IsSuccess)
                return new Result<bool>(currentResult.Error);

            var newBalance = currentResult.Data + amount;
            var today = DateTime.Today.ToString("yyyyMMdd");
            var key = string.Format(ACCOUNT_TODAY_BALANCE, today, source);
            await _db.StringSetAsync(key, newBalance.ToString(), TimeSpan.FromDays(7));
            return new Result<bool>(true);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error on {nameof} on account {source}", nameof(AddTransactionValueAsync), source);
            return new Result<bool>(new Error("Error while trying to add balance", 503));
        }
    }
}
