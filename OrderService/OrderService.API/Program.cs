using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OrderService.API.Middleware;
using OrderService.Application.Orders;
using OrderService.Application.Orders.Behaviors;
using OrderService.Application.Orders.Commands.CreateOrder;
using OrderService.Application.Orders.Interfaces;
using OrderService.Application.Catalog.Interfaces;
using OrderService.Infrastructure;
using OrderService.Infrastructure.Kafka;
using OrderService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("OrderDatabase");
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddMediatR(typeof(CreateOrderCommand).Assembly);
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderCommand>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddAutoMapper(typeof(OrdersMappingProfile).Assembly, typeof(Program).Assembly);

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICatalogProductsReadRepository, CatalogProductsReadRepository>();
builder.Services.AddSingleton<IOrderEventsPublisher, KafkaOrderEventsPublisher>();
builder.Services.AddTransient<ExceptionHandlingMiddleware>();
builder.Services.AddHostedService<ProductUpsertedConsumer>();
builder.Services.AddHostedService<OrderPaidConsumer>();
builder.Services.AddHostedService<OrderPaymentFailedConsumer>();

builder.Services.AddControllers();

builder.Services.AddHttpClient("CustomerService", client =>
{
    var baseUrl = builder.Configuration["Services:CustomerServiceBaseUrl"] ?? "http://localhost:5163";
    client.BaseAddress = new Uri(baseUrl);
});

var jwtSection = builder.Configuration.GetSection("Jwt");
var signingKey = jwtSection.GetValue<string>("SigningKey") ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtSection.GetValue<string>("Issuer"),
            ValidAudience = jwtSection.GetValue<string>("Audience"),
            IssuerSigningKey = key
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
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

