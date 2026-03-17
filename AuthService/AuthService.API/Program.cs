using System.Text;
using AuthService.Application.Auth.Commands.Login;
using AuthService.Application.Auth.Interfaces;
using AuthService.API.Middleware;
using AuthService.Application.Auth.Behaviors;
using AuthService.Infrastructure;
using AuthService.Infrastructure.Customers;
using AuthService.Infrastructure.Kafka;
using AuthService.Infrastructure.Security;
using AuthService.Infrastructure.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("AuthDatabase");
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddMediatR(typeof(LoginCommand).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(LoginCommand).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

builder.Services.AddHttpClient<ICustomerServiceClient, CustomerServiceClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5163");
});

builder.Services.AddHostedService<CustomerRegisteredConsumer>();

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

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
