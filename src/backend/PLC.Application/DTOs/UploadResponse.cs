namespace PLC.Application.DTOs;

public class UploadResponse
{
    public string UploadId { get; set; } = null!;
    public List<UploadedDocumentDto> Documents { get; set; } = new();
}

public class UploadedDocumentDto
{
    public string Id { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public long FileSize { get; set; }
    public string MimeType { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime UploadedAt { get; set; }
}
