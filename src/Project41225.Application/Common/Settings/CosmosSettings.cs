using System.ComponentModel.DataAnnotations;

namespace Project41225.Application.Common.Settings;

public class CosmosSettings
{
    public const string AppSettingsKey = "Cosmos";

    [Required(AllowEmptyStrings = false)]
    public string AccountEndpoint { get; set; } = string.Empty;
    
    [Required(AllowEmptyStrings = false)]
    public string AccountKey { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string DatabaseName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string ContainerName { get; set; } = string.Empty;
}
