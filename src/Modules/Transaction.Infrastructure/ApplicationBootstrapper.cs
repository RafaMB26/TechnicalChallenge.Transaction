using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Transaction.Application.Services;
using Transaction.Domain.Config;
using Transaction.Domain.Interfaces.Producers;
using Transaction.Domain.Interfaces.Repositories;
using Transaction.Domain.Interfaces.Services;
using Transaction.Ports.KafkaConsumers.Producers;
using Transaction.Ports.Postgres;
using Transaction.Ports.Postgres.Repositories;

namespace Transaction.Infrastructure;

public static class ApplicationBootstrapper
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITransactionService, TransactionService>();
    }
    public static void AddPortsServices(this IServiceCollection services, IConfiguration configuration)
    {
        var t = configuration.GetConnectionString("TransactionConnection");
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ITransactionStatusRepository, TransactionStatusRepository>();
        services.AddDbContext<TransactionDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("TransactionConnection"));
        });
        services.AddScoped<ITransactionProducer, TransactionProducer>();
    }
}