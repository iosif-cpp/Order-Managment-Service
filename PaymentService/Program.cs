using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PaymentService.API.Middleware;
using PaymentService.Application.Common.Behaviors;
using PaymentService.Application.Balances.Interfaces;
using PaymentService.Infrastructure;
using PaymentService.Infrastructure.Kafka;
using PaymentService.Infrastructure.Repositories;
using FluentValidation;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("PaymentDatabase");
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddMediatR(typeof(IBalanceRepository).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(IBalanceRepository).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddScoped<IBalanceRepository, BalanceRepository>();
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

builder.Services.AddHostedService<CustomerRegisteredConsumer>();
builder.Services.AddHostedService<OrderPaymentRequestedConsumer>();

builder.Services.AddControllers();

builder.Services.AddHttpClient("CustomerService", client =>
{
    var baseUrl = builder.Configuration["Services:CustomerServiceBaseUrl"] ?? "http://localhost:5163";
    client.BaseAddress = new Uri(baseUrl);
});

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "OrderManagement.Auth";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "OrderManagement.Api";
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"] ?? "CHANGE_ME_TO_A_LONG_RANDOM_SECRET_KEY_32_CHARS_MIN";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
