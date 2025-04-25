using Common.Result;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transaction.Application.Services;
using Transaction.Domain.Config;
using Transaction.Domain.DTOs;
using Transaction.Domain.Interfaces.Producers;
using Transaction.Domain.Interfaces.Repositories;
using Transaction.Domain.Interfaces.Services;
using Transaction.Infrastructure;
using Transaction.Infrastructure.Producers;
using Transaction.Infrastructure.Repositories;
using Transaction.Presentation.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionStatusRepository, TransactionStatusRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

builder.Services.AddHostedService<TransactionStatusEventConsumer>();

builder.Services.AddSingleton<ITransactionProducer, TransactionProducer>();

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

builder.Services.AddDbContext<TransactionDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("TransactionConnection"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/{externalId:guid}", async ([FromServices] ITransactionService transactionService, Guid externalId) =>
{
    if (externalId == Guid.Empty)
        return Results.BadRequest(new Error("The transaction external id is incorrect", 400));

    var transactionResult = await transactionService.GetTransactionByExternalIdAsync(externalId);

    if (!transactionResult.IsSuccess && (transactionResult.Error.Code >= 400 || transactionResult.Error.Code < 500))
        return Results.BadRequest(transactionResult.Error);
    else if (!transactionResult.IsSuccess && transactionResult.Error.Code >= 500)
        return Results.Problem(transactionResult.Error.Message);

    return Results.Ok(transactionResult.Data);
}).WithName("Get transaction by external id")
.WithOpenApi();

app.MapPost("/", async ([FromServices] ITransactionService transactionService, [FromBody] CreateTransactionDTO transaction) =>
{
    var transactionCreationResult = await transactionService.SendTransactionAsync(transaction);

    if (!transactionCreationResult.IsSuccess && (transactionCreationResult.Error.Code >= 400 || transactionCreationResult.Error.Code < 500))
        return Results.BadRequest(transactionCreationResult.Error);
    else if(!transactionCreationResult.IsSuccess && transactionCreationResult.Error.Code >= 500)
        return Results.Problem(transactionCreationResult.Error.Message);

    return Results.Ok(transactionCreationResult.Data);

}).WithName("Create transaction")
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
