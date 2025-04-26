using Azure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.Identity.Web;
using Project41225.Application.Configuration;
using Project41225.Application.Extensions;
using Project41225.Application.Interfaces;
using Project41225.Infrastructure.Extensions;
using Project41225.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi()
            .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
            .AddInMemoryTokenCaches();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Key Vault
builder.Services.Configure<KeyVaultSettings>(builder.Configuration.GetSection("KeyVault"));

// Custom services
builder.Services.AddScoped<IKeyVaultService, KeyVaultService>();

builder.Services.Configure<AzureOpenAISettings>(
    builder.Configuration.GetSection("AzureOpenAI"));

builder.Services.AddScoped<IAzureOpenAIService, AzureOpenAIService>();

// Application services
builder.Services.AddApplicationServices();

// Development-specific configuration
if (builder.Environment.IsDevelopment())
{
    // Cosmos DB
    builder.Services.AddInfrastructureServices(builder.Configuration);
}
else
{
    // Production-specific configuration
};

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
