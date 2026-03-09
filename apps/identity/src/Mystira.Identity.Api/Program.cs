using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Mystira.App.Application;
using Mystira.App.Application.CQRS.Auth.Commands;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.Ports.Services;
using Mystira.App.Application.Services;
using Mystira.App.Infrastructure.Azure;
using Mystira.App.Infrastructure.Data;
using Mystira.App.Infrastructure.Data.Repositories;
using Mystira.App.Infrastructure.Data.UnitOfWork;
using Mystira.Identity.Api.Services;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var cosmosConnectionString = builder.Configuration.GetConnectionString("CosmosDb");
if (!string.IsNullOrWhiteSpace(cosmosConnectionString))
{
    builder.Services.AddDbContext<MystiraAppDbContext>(options =>
        options.UseCosmos(cosmosConnectionString, "MystiraAppDb"));
}
else
{
    builder.Services.AddDbContext<MystiraAppDbContext>(options =>
        options.UseInMemoryDatabase("MystiraIdentityInMemoryDb"));
}

builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<MystiraAppDbContext>());

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IPendingSignupRepository, PendingSignupRepository>();
builder.Services.AddScoped<Mystira.Shared.Data.Repositories.IUnitOfWork, UnitOfWork>();

builder.Services.AddAzureEmailService(builder.Configuration);
builder.Services.AddSingleton<MagicSignupEmailBuilder>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IIdentityTokenService, IdentityTokenService>();

// Add Entra provisioning services
builder.Services.AddSingleton<IProvisioningQueue, InMemoryProvisioningQueue>();
builder.Services.AddScoped<IEntraProvisioningService, EntraProvisioningService>();
builder.Services.AddHostedService<ProvisioningBackgroundWorker>();

var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "mystira-identity-api";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "mystira-platform";
var jwtRsaPrivateKey = builder.Configuration["JwtSettings:RsaPrivateKey"];
var jwtRsaPublicKey = builder.Configuration["JwtSettings:RsaPublicKey"];
var jwtKey = builder.Configuration["JwtSettings:SecretKey"];

if (string.IsNullOrWhiteSpace(jwtRsaPrivateKey) && string.IsNullOrWhiteSpace(jwtKey))
{
    if (builder.Environment.IsDevelopment())
    {
        jwtKey = $"IdentityDevKey-{Guid.NewGuid():N}-{DateTime.UtcNow:yyyyMMdd}";
        builder.Configuration["JwtSettings:SecretKey"] = jwtKey;
    }
    else
    {
        throw new InvalidOperationException("JWT signing key not configured. Set JwtSettings:RsaPrivateKey or JwtSettings:SecretKey.");
    }
}

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        if (!string.IsNullOrWhiteSpace(jwtRsaPublicKey))
        {
            using var rsa = System.Security.Cryptography.RSA.Create();
            rsa.ImportFromPem(jwtRsaPublicKey);
            tokenValidationParameters.IssuerSigningKey = new RsaSecurityKey(rsa.ExportParameters(false));
        }
        else if (!string.IsNullOrWhiteSpace(jwtRsaPrivateKey))
        {
            using var rsa = System.Security.Cryptography.RSA.Create();
            rsa.ImportFromPem(jwtRsaPrivateKey);
            tokenValidationParameters.IssuerSigningKey = new RsaSecurityKey(rsa.ExportParameters(false));
        }
        else if (!string.IsNullOrWhiteSpace(jwtKey))
        {
            tokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        }

        options.TokenValidationParameters = tokenValidationParameters;
    });

builder.Services.AddAuthorization();
builder.Services.AddApplicationServices();

builder.Host.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(typeof(BootstrapAccountCommand).Assembly);
    opts.Policies.UseDurableLocalQueues();
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

var initializeDbOnStartup = builder.Configuration.GetValue("InitializeDatabaseOnStartup", true);
if (initializeDbOnStartup)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MystiraAppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();

namespace Mystira.Identity.Api
{
    public partial class Program;
}
