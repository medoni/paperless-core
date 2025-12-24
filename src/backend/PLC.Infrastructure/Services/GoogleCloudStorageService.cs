using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;
using PLC.Infrastructure.Configuration;
using PLC.Infrastructure.Interfaces;

namespace PLC.Infrastructure.Services;

/// <summary>
/// Google Cloud Storage implementation for document storage
/// </summary>
public class GoogleCloudStorageService : IDocumentStorageService
{
    private readonly StorageClient _storageClient;
    private readonly GoogleCloudOptions _options;

    public GoogleCloudStorageService(
        StorageClient storageClient,
        IOptions<GoogleCloudOptions> options)
    {
        _storageClient = storageClient;
        _options = options.Value;
    }

    public async Task<string> UploadAsync(
        string documentId,
        string fileName,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        // Build storage path: documents/{year}/{month}/{documentId}.{ext}
        var now = DateTime.UtcNow;
        var extension = Path.GetExtension(fileName);
        var objectName = $"documents/{now.Year:D4}/{now.Month:D2}/{documentId}{extension}";

        await _storageClient.UploadObjectAsync(
            _options.StorageBucket,
            objectName,
            contentType,
            fileStream,
            cancellationToken: cancellationToken);

        // Return GCS URI
        return $"gs://{_options.StorageBucket}/{objectName}";
    }

    public async Task<Stream> DownloadAsync(
        string storageUri,
        CancellationToken cancellationToken = default)
    {
        // Parse gs://bucket/object/path
        var uri = new Uri(storageUri);
        var bucket = uri.Host;
        var objectName = uri.AbsolutePath.TrimStart('/');

        var memoryStream = new MemoryStream();
        await _storageClient.DownloadObjectAsync(
            bucket,
            objectName,
            memoryStream,
            cancellationToken: cancellationToken);

        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task DeleteAsync(
        string storageUri,
        CancellationToken cancellationToken = default)
    {
        // Parse gs://bucket/object/path
        var uri = new Uri(storageUri);
        var bucket = uri.Host;
        var objectName = uri.AbsolutePath.TrimStart('/');

        await _storageClient.DeleteObjectAsync(
            bucket,
            objectName,
            cancellationToken: cancellationToken);
    }

    public async Task<string> GetSignedUrlAsync(
        string storageUri,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        // Parse gs://bucket/object/path
        var uri = new Uri(storageUri);
        var bucket = uri.Host;
        var objectName = uri.AbsolutePath.TrimStart('/');

        var urlSigner = UrlSigner.FromCredentialFile(
            Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS"));

        var signedUrl = await urlSigner.SignAsync(
            bucket,
            objectName,
            expiration,
            cancellationToken: cancellationToken);

        return signedUrl;
    }
}
