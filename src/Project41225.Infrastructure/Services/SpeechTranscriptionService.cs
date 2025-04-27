using Microsoft.AspNetCore.SignalR;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Options;
using Project41225.Application.Common.Settings;
using Project41225.Application.Interfaces;
using Project41225.Infrastructure.Hubs;
using System.Collections.Concurrent;

namespace Project41225.Infrastructure.Services;

public class SpeechTranscriptionService : ISpeechTranscriptionService
{
    private readonly SpeechTranscriptionSettings _settings;
    private readonly IHubContext<AudioHub> _hubContext;

    // Map connectionId to recognizer
    // thread-safe collections (ConcurrentDictionary) to store the SpeechRecognizer and PushAudioInputStream instances per connection
    private readonly ConcurrentDictionary<string, SpeechRecognizer> _recognizers = new();
    private readonly ConcurrentDictionary<string, PushAudioInputStream> _pushStreams = new();

    public SpeechTranscriptionService(
        IOptions<SpeechTranscriptionSettings> opts,
        IHubContext<AudioHub> hubContext)
    {
        _settings = opts.Value;
        _hubContext = hubContext;
    }

    public async Task StartRecognitionAsync(string connectionId, Guid sessionId)
    {
        var config = SpeechConfig.FromSubscription(_settings.SubscriptionKey, _settings.Region);
        config.SpeechRecognitionLanguage = _settings.Language;

        // Create and store the push-stream
        var pushStream = AudioInputStream.CreatePushStream();
        _pushStreams[connectionId] = pushStream;

        // Create audio config from that stream
        var audioConfig = AudioConfig.FromStreamInput(pushStream);

        var recognizer = new SpeechRecognizer(config, audioConfig);

        recognizer.Recognizing += async (s, e) =>
            await _hubContext.Clients.Client(connectionId)
                .SendAsync("ReceivePartialTranscript", sessionId, e.Result.Text);

        recognizer.Recognized += async (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.Result.Text))
            {
                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("ReceiveFinalTranscript", sessionId, e.Result.Text);
            }
        };

        _recognizers[connectionId] = recognizer;
        await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
    }

    public Task ProcessAudioChunkAsync(string connectionId, byte[] audioChunk)
    {
        // Fetch the stored push-stream and write into it
        if (_pushStreams.TryGetValue(connectionId, out var stream))
        {
            stream.Write(audioChunk);
        }
        return Task.CompletedTask;
    }

    public async Task StopRecognitionAsync(string connectionId)
    {
        if (_recognizers.TryRemove(connectionId, out var recognizer))
        {
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            recognizer.Dispose();
        }

        if (_pushStreams.TryRemove(connectionId, out var stream))
        {
            stream.Close();
        }
    }
}