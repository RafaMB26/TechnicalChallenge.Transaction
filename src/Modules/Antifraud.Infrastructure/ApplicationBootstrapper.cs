using Antifraud.Adapter.KafkaProducer.Producers;
using Antifraud.Application.Services;
using Antifraud.Domain.Config;
using Antifraud.Domain.Interfaces.Producer;
using Antifraud.Domain.Interfaces.Repositories;
using Antifraud.Domain.Interfaces.Services;
using Antifraud.Ports.Redis.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Antifraud.Infrastructure;

public static class ApplicationBootstrapper
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAntifraudService, AntifraudService>();
    }

    public static void AddPortsServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.AddScoped<IAntifraudProducer, AntifraudProducer>();
    }
}
