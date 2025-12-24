using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using PLC.Domain.Events;
using PLC.Infrastructure.Configuration;
using PLC.Infrastructure.Interfaces;
using System.Text.Json;

namespace PLC.Infrastructure.Services;

/// <summary>
/// Google Pub/Sub implementation for event publishing
/// </summary>
public class GooglePubSubPublisher : IEventPublisher
{
    private readonly PublisherClient _publisherClient;
    private readonly GoogleCloudOptions _options;

    public GooglePubSubPublisher(
        PublisherClient publisherClient,
        IOptions<GoogleCloudOptions> options)
    {
        _publisherClient = publisherClient;
        _options = options.Value;
    }

    public async Task PublishDocumentChangedAsync(
        DocumentChangedEvent @event,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(@event);
        var message = new PubsubMessage
        {
            Data = ByteString.CopyFromUtf8(json),
            Attributes =
            {
                ["documentId"] = @event.DocumentId,
                ["changeType"] = @event.ChangeType,
                ["timestamp"] = @event.Timestamp.ToString("O")
            }
        };

        await _publisherClient.PublishAsync(message);
    }
}
