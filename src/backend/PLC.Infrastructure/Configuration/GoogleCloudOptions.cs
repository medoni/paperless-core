namespace PLC.Infrastructure.Configuration;

/// <summary>
/// Configuration options for Google Cloud services
/// </summary>
public class GoogleCloudOptions
{
    public const string SectionName = "GoogleCloud";

    public string ProjectId { get; set; } = null!;
    public string StorageBucket { get; set; } = null!;
    public string FirestoreDatabase { get; set; } = "(default)";
    public string FirestoreCollection { get; set; } = "plc-documents";
    public string PubSubTopic { get; set; } = "document-changed";
}
