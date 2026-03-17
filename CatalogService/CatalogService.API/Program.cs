using System.Text;
using CatalogService.API.Middleware;
using CatalogService.Application.Products;
using CatalogService.Application.Products.Behaviors;
using CatalogService.Application.Products.Commands.CreateProduct;
using CatalogService.Application.Products.Interfaces;
using CatalogService.Infrastructure.Kafka;
using CatalogService.Application.Categories;
using CatalogService.Application.Categories.Interfaces;
using CatalogService.Infrastructure;
using CatalogService.Infrastructure.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("CatalogDatabase");
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddMediatR(typeof(CreateProductCommand).Assembly);
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductCommand>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddAutoMapper(
    typeof(ProductsMappingProfile).Assembly,
    typeof(CategoriesMappingProfile).Assembly,
    typeof(Program).Assembly);

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddSingleton<IProductEventsPublisher, KafkaProductEventsPublisher>();
builder.Services.AddTransient<ExceptionHandlingMiddleware>();
builder.Services.AddHostedService<OrderPaidConsumer>();

builder.Services.AddControllers();

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
