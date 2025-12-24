using PLC.Domain;

namespace PLC.Infrastructure.Interfaces;

/// <summary>
/// Repository for Document persistence (Firestore)
/// </summary>
public interface IDocumentRepository
{
    /// <summary>
    /// Save a new document
    /// </summary>
    Task<Document> CreateAsync(Document document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get document by ID
    /// </summary>
    Task<Document?> GetByIdAsync(string documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing document
    /// Idempotent operation
    /// </summary>
    Task UpdateAsync(Document document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if document exists
    /// </summary>
    Task<bool> ExistsAsync(string documentId, CancellationToken cancellationToken = default);
}
