using PLC.Application.Commands;
using PLC.Application.DTOs;
using PLC.Domain;
using PLC.Domain.Events;
using PLC.Infrastructure.Interfaces;

namespace PLC.Application.Handlers;

/// <summary>
/// Handler for UploadDocumentCommand
/// Orchestrates: Validation -> Storage -> Repository -> Event Publishing
/// </summary>
public class UploadDocumentHandler
{
    private readonly IDocumentStorageService _storageService;
    private readonly IDocumentRepository _documentRepository;
    private readonly IEventPublisher _eventPublisher;

    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB
    private const int MaxBatchSize = 100;
    private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
    private static readonly string[] AllowedMimeTypes =
    {
        "application/pdf",
        "image/jpeg",
        "image/jpg",
        "image/png"
    };

    public UploadDocumentHandler(
        IDocumentStorageService storageService,
        IDocumentRepository documentRepository,
        IEventPublisher eventPublisher)
    {
        _storageService = storageService;
        _documentRepository = documentRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<UploadResponse> HandleAsync(
        UploadDocumentCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validate batch size
        if (command.Files.Length == 0)
            throw new ArgumentException("No files provided");

        if (command.Files.Length > MaxBatchSize)
            throw new ArgumentException($"Maximum {MaxBatchSize} files allowed per upload");

        // Generate upload ID
        var uploadId = Guid.NewGuid().ToString();
        var uploadedDocuments = new List<UploadedDocumentDto>();

        // Process each file
        foreach (var file in command.Files)
        {
            // Validate individual file
            ValidateFile(file);

            // Create document entity
            var document = Document.Create(
                fileName: file.FileName,
                fileSize: file.Length,
                mimeType: file.ContentType,
                storageUri: string.Empty, // Will be set after upload
                uploadedBy: command.UploadedBy,
                tags: command.Tags,
                assignedTo: command.AssignedTo
            );

            // Upload to storage
            using var stream = file.OpenReadStream();
            var storageUri = await _storageService.UploadAsync(
                document.Id,
                file.FileName,
                stream,
                file.ContentType,
                cancellationToken);

            // Update storage URI (use reflection to set private property)
            SetStorageUri(document, storageUri);

            // Save to repository
            await _documentRepository.CreateAsync(document, cancellationToken);

            // Publish event (workers will process)
            await _eventPublisher.PublishDocumentChangedAsync(
                DocumentChangedEvent.Uploaded(document.Id),
                cancellationToken);

            // Add to response
            uploadedDocuments.Add(new UploadedDocumentDto
            {
                Id = document.Id,
                FileName = document.FileName,
                FileSize = document.FileSize,
                MimeType = document.MimeType,
                Status = document.Status.ToString(),
                UploadedAt = document.UploadedAt
            });
        }

        return new UploadResponse
        {
            UploadId = uploadId,
            Documents = uploadedDocuments
        };
    }

    private void ValidateFile(Microsoft.AspNetCore.Http.IFormFile file)
    {
        // Check file size
        if (file.Length > MaxFileSizeBytes)
            throw new ArgumentException(
                $"File '{file.FileName}' exceeds maximum size of {MaxFileSizeBytes / 1024 / 1024} MB");

        // Check extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException(
                $"File '{file.FileName}' has unsupported format. Allowed: {string.Join(", ", AllowedExtensions)}");

        // Check MIME type
        if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            throw new ArgumentException(
                $"File '{file.FileName}' has unsupported MIME type '{file.ContentType}'");
    }

    private void SetStorageUri(Document document, string storageUri)
    {
        var prop = document.GetType().GetProperty("StorageUri",
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        prop?.SetValue(document, storageUri);
    }
}
