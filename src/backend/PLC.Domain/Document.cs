namespace PLC.Domain;

/// <summary>
/// Document aggregate root
/// Represents a single document in the system with its metadata and processing state
/// </summary>
public class Document
{
    // Identity
    public string Id { get; private set; } = null!;

    // File Metadata
    public string FileName { get; private set; } = null!;
    public long FileSize { get; private set; }
    public string MimeType { get; private set; } = null!;
    public string StorageUri { get; private set; } = null!;

    // Upload Info
    public DateTime UploadedAt { get; private set; }
    public string UploadedBy { get; private set; } = null!;

    // Status
    public DocumentStatus Status { get; private set; }

    // Classification (null if not yet classified)
    public string? Category { get; private set; }
    public string? Subcategory { get; private set; }
    public double? ClassificationConfidence { get; private set; }

    // Slug (generated after classification, or fallback)
    public string? Slug { get; private set; }

    // OCR (null if not yet processed or not required)
    public string? OcrText { get; private set; }

    // Extracted Data (null if not yet processed or not required)
    // Stored as JSON string for flexibility
    public string? ExtractedDataJson { get; private set; }

    // User Metadata
    public List<string> Tags { get; private set; } = new();
    public List<string> AssignedTo { get; private set; } = new();

    // Timestamps
    public DateTime? ClassifiedAt { get; private set; }
    public DateTime? OcrCompletedAt { get; private set; }
    public DateTime? ExtractionCompletedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    // Private constructor for ORM/Serialization
    private Document() { }

    /// <summary>
    /// Create a new document after successful upload
    /// </summary>
    public static Document Create(
        string fileName,
        long fileSize,
        string mimeType,
        string storageUri,
        string uploadedBy,
        List<string>? tags = null,
        List<string>? assignedTo = null)
    {
        var document = new Document
        {
            Id = Guid.NewGuid().ToString(),
            FileName = fileName,
            FileSize = fileSize,
            MimeType = mimeType,
            StorageUri = storageUri,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = uploadedBy,
            Status = DocumentStatus.Processing, // Start in Processing after upload
            Tags = tags ?? new List<string>(),
            AssignedTo = assignedTo ?? new List<string>()
        };

        return document;
    }

    /// <summary>
    /// Update classification information (called by Classifier Worker)
    /// </summary>
    public void UpdateClassification(string category, string subcategory, double confidence)
    {
        if (Status == DocumentStatus.Deleted)
            throw new InvalidOperationException("Cannot update deleted document");

        Category = category;
        Subcategory = subcategory;
        ClassificationConfidence = confidence;
        ClassifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set the document slug (called after classification or as fallback)
    /// </summary>
    public void SetSlug(string slug)
    {
        if (Status == DocumentStatus.Deleted)
            throw new InvalidOperationException("Cannot update deleted document");

        Slug = slug;
    }

    /// <summary>
    /// Update OCR text (called by OCR Worker)
    /// </summary>
    public void UpdateOcrText(string ocrText)
    {
        if (Status == DocumentStatus.Deleted)
            throw new InvalidOperationException("Cannot update deleted document");

        OcrText = ocrText;
        OcrCompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update extracted data (called by Extraction Worker)
    /// </summary>
    public void UpdateExtractedData(string extractedDataJson)
    {
        if (Status == DocumentStatus.Deleted)
            throw new InvalidOperationException("Cannot update deleted document");

        ExtractedDataJson = extractedDataJson;
        ExtractionCompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark document as finished (all processing completed)
    /// </summary>
    public void MarkAsFinished()
    {
        if (Status == DocumentStatus.Deleted)
            throw new InvalidOperationException("Cannot finish deleted document");

        Status = DocumentStatus.Finished;
        FinishedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Soft-delete the document
    /// </summary>
    public void Delete()
    {
        Status = DocumentStatus.Deleted;
        DeletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if document needs classification
    /// </summary>
    public bool NeedsClassification() => Category == null;

    /// <summary>
    /// Check if document needs OCR processing
    /// </summary>
    public bool NeedsOcr(bool ocrEnabled) =>
        Category != null && ocrEnabled && OcrText == null;

    /// <summary>
    /// Check if document needs data extraction
    /// </summary>
    public bool NeedsExtraction(bool extractionEnabled) =>
        Category != null && extractionEnabled && ExtractedDataJson == null;

    /// <summary>
    /// Check if all required processing is complete
    /// </summary>
    public bool IsProcessingComplete(bool ocrRequired, bool extractionRequired)
    {
        // Classification and Slug are always required
        if (Category == null || Slug == null)
            return false;

        // Check OCR if required
        if (ocrRequired && OcrText == null)
            return false;

        // Check Extraction if required
        if (extractionRequired && ExtractedDataJson == null)
            return false;

        return true;
    }
}
