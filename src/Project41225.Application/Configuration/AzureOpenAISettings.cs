namespace Project41225.Application.Configuration;
public class AzureOpenAISettings
{
    public const string AppSettingsKey = "AzureOpenAI";
    public string Key { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = string.Empty;
}
