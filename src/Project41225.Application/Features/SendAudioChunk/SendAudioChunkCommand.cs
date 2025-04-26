using MediatR;

namespace Project41225.Application.Features.SendAudioChunk;

public class SendAudioChunkCommand : IRequest<Unit>
{
    public SendAudioChunkCommand(Guid sessionId, byte[] data)
    {
        SessionId = sessionId;
        Data = data;
    }

    public Guid SessionId { get; }
    public byte[] Data { get; }
}
