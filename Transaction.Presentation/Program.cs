using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transaction.Application.Services;
using Transaction.Domain.DTOs;
using Transaction.Domain.Interfaces.Repositories;
using Transaction.Domain.Interfaces.Services;
using Transaction.Infrastructure;
using Transaction.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionStatusRepository, TransactionStatusRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
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


app.MapPost("/transaction", ([FromBody] CreateTransactionDTO transaction) =>
{

}).WithName("Create transaction")
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
