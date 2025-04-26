using System.ComponentModel.DataAnnotations;

namespace Project41225.Application.Common.Settings;

public class SpeechTranscriptionSettings
{
    public const string AppSettingsKey = "SpeechTranscription";

    [Required]
    public string SubscriptionKey { get; set; } = string.Empty;

    [Required]
    public string Region { get; set; } = string.Empty;

    [Required]
    public string Language { get; set; } = "en-US";
}
