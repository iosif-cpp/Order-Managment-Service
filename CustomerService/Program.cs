using CustomerService.API.Middleware;
using CustomerService.Application.Users.Commands.RegisterUser;
using CustomerService.Application.Users.Interfaces;
using CustomerService.Infrastructure;
using CustomerService.Infrastructure.Kafka;
using CustomerService.Infrastructure.Repositories;
using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("CustomerDatabase");
builder.Services.AddDbContext<CustomerDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddMediatR(typeof(RegisterUserCommand).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(RegisterUserCommand).Assembly);

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddSingleton<ICustomerEventsPublisher, KafkaCustomerEventsPublisher>();
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();
