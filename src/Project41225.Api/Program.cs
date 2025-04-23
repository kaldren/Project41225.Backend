using Azure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.Identity.Web;
using Project41225.Application.Configuration;
using Project41225.Application.Interfaces;
using Project41225.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi()
            .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
            .AddInMemoryTokenCaches();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Key Vault
builder.Services.Configure<KeyVaultSettings>(builder.Configuration.GetSection("KeyVault"));

// Custom services
builder.Services.AddScoped<IKeyVaultService, KeyVaultService>();

builder.Services.Configure<AzureOpenAISettings>(
    builder.Configuration.GetSection("AzureOpenAI"));

builder.Services.AddScoped<IAzureOpenAIService, AzureOpenAIService>();

// Cosmos DB
builder.Services.AddSingleton(s =>
{
    var cosmosClient = new CosmosClient(accountEndpoint: "", tokenCredential: new DefaultAzureCredential());
    return cosmosClient.GetDatabase("InterviewDB");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
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
