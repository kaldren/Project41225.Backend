using System.ComponentModel.DataAnnotations;

namespace Project41225.Application.Common.Settings;
public class AzureOpenAISettings
{
    public const string AppSettingsKey = "AzureOpenAI";
    
    [Required(AllowEmptyStrings = false)]
    public string Key { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Endpoint { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string ModelDeploymentName { get; set; } = string.Empty;

    [Required]
    public AzureOpenAIRealtimeSettings RealtimeApi { get; set; } = new();

    public class AzureOpenAIRealtimeSettings
    {
        [Required(AllowEmptyStrings = false)]
        public string WebSocketEndpoint { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        public string ModelDeploymentName { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        public string AudioFormat { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        public string AudioTranscriptionModel { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        public string TranscriptionLanguage { get; set; } = string.Empty;
    }
}
