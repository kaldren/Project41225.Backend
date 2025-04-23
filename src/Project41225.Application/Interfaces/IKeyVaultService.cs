namespace Project41225.Application.Interfaces;
public interface IKeyVaultService
{
    Task<string?> GetSecretAsync(string secretName);
}
