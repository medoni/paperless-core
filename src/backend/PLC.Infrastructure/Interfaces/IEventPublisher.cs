using PLC.Domain.Events;

namespace PLC.Infrastructure.Interfaces;

/// <summary>
/// Event publisher for DocumentChangedEvent (Google Pub/Sub)
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publish a DocumentChangedEvent to Pub/Sub
    /// All workers listen to this event and process idempotently
    /// </summary>
    Task PublishDocumentChangedAsync(
        DocumentChangedEvent @event,
        CancellationToken cancellationToken = default);
}
