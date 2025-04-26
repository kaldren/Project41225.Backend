namespace Project41225.Application.Interfaces;
public interface IKeyVaultProvider
{
    Task<string?> GetSecretAsync(string secretName);
}
