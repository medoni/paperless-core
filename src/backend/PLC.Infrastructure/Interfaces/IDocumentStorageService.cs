namespace PLC.Infrastructure.Interfaces;

/// <summary>
/// Service for storing document files (Google Cloud Storage)
/// </summary>
public interface IDocumentStorageService
{
    /// <summary>
    /// Upload a document file to storage
    /// Returns the storage URI (e.g., gs://bucket/path/to/file.pdf)
    /// </summary>
    Task<string> UploadAsync(
        string documentId,
        string fileName,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Download a document file from storage
    /// </summary>
    Task<Stream> DownloadAsync(
        string storageUri,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a document file from storage
    /// </summary>
    Task DeleteAsync(
        string storageUri,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get signed URL for direct download (future use). TODO: Check if it's really needed.
    /// </summary>
    Task<string> GetSignedUrlAsync(
        string storageUri,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);
}
