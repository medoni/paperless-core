namespace PLC.Application.DTOs;

public class DocumentDto
{
    public string Id { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public long FileSize { get; set; }
    public string MimeType { get; set; } = null!;
    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = null!;
    public string Status { get; set; } = null!;

    // Classification
    public string? Category { get; set; }
    public string? Subcategory { get; set; }
    public double? ClassificationConfidence { get; set; }

    // Slug
    public string? Slug { get; set; }

    // User metadata
    public List<string> Tags { get; set; } = new();
    public List<string> AssignedTo { get; set; } = new();

    // Storage
    public string StorageUri { get; set; } = null!;

    // Optional processed data
    public string? OcrText { get; set; }
    public string? ExtractedDataJson { get; set; }
}
