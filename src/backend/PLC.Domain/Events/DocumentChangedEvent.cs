namespace PLC.Domain.Events;

/// <summary>
/// Event published whenever a document is created or updated
/// Workers listen to this event and check if they need to process the document
/// </summary>
public class DocumentChangedEvent
{
    public string DocumentId { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public string ChangeType { get; set; } = null!; // TODO: Enum, "uploaded", "updated", "deleted"

    public static DocumentChangedEvent Uploaded(string documentId)
    {
        return new DocumentChangedEvent
        {
            DocumentId = documentId,
            Timestamp = DateTime.UtcNow,
            ChangeType = "uploaded"
        };
    }

    public static DocumentChangedEvent Updated(string documentId)
    {
        return new DocumentChangedEvent
        {
            DocumentId = documentId,
            Timestamp = DateTime.UtcNow,
            ChangeType = "updated"
        };
    }

    public static DocumentChangedEvent Deleted(string documentId)
    {
        return new DocumentChangedEvent
        {
            DocumentId = documentId,
            Timestamp = DateTime.UtcNow,
            ChangeType = "deleted"
        };
    }
}
