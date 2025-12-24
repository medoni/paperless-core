using Microsoft.AspNetCore.Http;

namespace PLC.Application.Commands;

/// <summary>
/// Command to upload one or more documents
/// </summary>
public class UploadDocumentCommand
{
    public IFormFile[] Files { get; set; } = Array.Empty<IFormFile>();
    public string UploadedBy { get; set; } = "anonymous"; // TODO: Replace with actual user ID from auth
    public List<string>? Tags { get; set; }
    public List<string>? AssignedTo { get; set; }
}
