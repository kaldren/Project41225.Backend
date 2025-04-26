using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;
using Project41225.Application.Configuration;
using Project41225.Application.Interfaces;

namespace Project41225.Infrastructure.Services;
public class KeyVaultService : IKeyVaultProvider
{
    private readonly IOptions<KeyVaultSettings> _options;
    private readonly SecretClient _secretClient;

    public KeyVaultService(IOptions<KeyVaultSettings> options)
    {
        _options = options;

        if (string.IsNullOrEmpty(_options.Value.VaultUri))
        {
            throw new ArgumentException("Key Vault URI is not configured.");
        }

        // Initialize the Key Vault client here if needed
        // For example, using Azure.Identity to authenticate

        var credential = new DefaultAzureCredential();
        var client = new SecretClient(new Uri(_options.Value.VaultUri), credential);
        _secretClient = client;
    }

    public async Task<string?> GetSecretAsync(string secretName)
    {
        var secret = await _secretClient.GetSecretAsync(secretName);

        // Todo: Handle the case when the secret is not found with a custom exception named KeyVaultSecretNotFoundException

        return secret.Value.Value;

        throw new NotImplementedException();
    }
}
