# PaperlessCore - Data Model

## Overview

Dieses Dokument beschreibt das Domain Model von PaperlessCore.

## Core Entities

### Document

Zentrale Entity für jedes hochgeladene Dokument.

```csharp
namespace PLC.Domain.Entities;

public class Document
{
    // Identity
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }

    // Upload Information
    public string FileName { get; set; }
    public long FileSizeBytes { get; init; }
    public string MimeType { get; init; }
    public string StorageUri { get; init; }  // gs://... or s3://...
    public Guid UploadedBy { get; init; }    // User ID

    // Classification
    public string? CategoryId { get; set; }          // From YAML (e.g., "work-income")
    public string? SubcategoryId { get; set; }       // From YAML (e.g., "salary-statement")
    public double? ClassificationConfidence { get; set; }
    public ClassificationStatus ClassificationStatus { get; set; }

    // Person Assignment (Hybrid Model)
    public List<string> AssignedTo { get; set; } = new();  // ["Micha"], ["Anika", "Micha"], or []

    // Tags (Flexible)
    public List<string> Tags { get; set; } = new();  // ["2024", "important", "tax"]

    // Temporal
    public int? Year { get; set; }       // Extracted or manual
    public int? Month { get; set; }      // Optional
    public int? Quarter { get; set; }    // Optional
    public DateTime? DocumentDate { get; set; }  // Date ON the document

    // OCR
    public OcrResult? OcrResult { get; set; }
    public OcrStatus OcrStatus { get; set; }

    // Extraction
    public ExtractedData? ExtractedData { get; set; }
    public ExtractionStatus ExtractionStatus { get; set; }

    // Overall Status
    public ProcessingStatus ProcessingStatus { get; set; }

    // Retention
    public int? RetentionYears { get; set; }    // From YAML config
    public DateTime? DeleteAfter { get; set; }  // Calculated: CreatedAt + RetentionYears

    // Metadata
    public Dictionary<string, string> CustomMetadata { get; set; } = new();
}
```

### OcrResult (Value Object)

```csharp
namespace PLC.Domain.ValueObjects;

public record OcrResult
{
    public string FullText { get; init; }
    public double Confidence { get; init; }
    public string Language { get; init; }
    public int PageCount { get; init; }
    public DateTime ProcessedAt { get; init; }
    public string Provider { get; init; }  // "google-document-ai" or "aws-textract"
    public decimal CostCents { get; init; }
}
```

### ExtractedData (Polymorphic)

Base class for document-type-specific extracted data.

```csharp
namespace PLC.Domain.ValueObjects;

public abstract record ExtractedData
{
    public DateTime ExtractedAt { get; init; }
    public double Confidence { get; init; }
}

// Receipt-specific
public record ReceiptData : ExtractedData
{
    public DateTime? Date { get; init; }
    public string? Merchant { get; init; }
    public decimal? TotalAmount { get; init; }
    public string? Currency { get; init; } = "EUR";
    public List<VatRate>? VatRates { get; init; }
    public string? PaymentMethod { get; init; }  // "cash", "card", etc.
    public List<LineItem>? LineItems { get; init; }  // Optional, detailed
}

public record VatRate(int Rate, decimal Amount);

public record LineItem(string Description, decimal Amount, int? Quantity = null);

// Invoice-specific
public record InvoiceData : ExtractedData
{
    public string? InvoiceNumber { get; init; }
    public DateTime? Date { get; init; }
    public DateTime? DueDate { get; init; }
    public string? Vendor { get; init; }
    public decimal? TotalAmount { get; init; }
    public decimal? VatAmount { get; init; }
    public string? Currency { get; init; } = "EUR";
}

// Tax Assessment
public record TaxAssessmentData : ExtractedData
{
    public int TaxYear { get; init; }
    public string? TaxNumber { get; init; }
    public decimal? TotalTaxAmount { get; init; }
    public decimal? RefundAmount { get; init; }
}

// Extendable for other document types...
```

### Category & Subcategory (Read from YAML)

Not stored as entities, read from `document-structure.yaml` at runtime.

```csharp
namespace PLC.Domain.Models;

public record CategoryDefinition
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string NameEn { get; init; }
    public AssignmentMode AssignmentMode { get; init; }
    public List<SubcategoryDefinition> Subcategories { get; init; }
}

public record SubcategoryDefinition
{
    public string Id { get; init; }
    public string Name { get; init; }
    public bool OcrEnabled { get; init; }
    public bool FullTextSearch { get; init; }
    public bool DataExtraction { get; init; }
    public int RetentionYears { get; init; }
    public List<string> RequiredTags { get; init; }
    public List<string>? ExtractionFields { get; init; }
    public AssignmentMode? AssignmentMode { get; init; }  // Override category
}
```

### User (simplified for V1)

```csharp
namespace PLC.Domain.Entities;

public class User
{
    public Guid Id { get; init; }
    public string Email { get; init; }
    public string DisplayName { get; init; }
    public DateTime CreatedAt { get; init; }

    // V1: Single household, no multi-tenancy
    // V2: Add HouseholdId for multi-household support
}
```

## Enums

```csharp
namespace PLC.Domain.Enums;

public enum ProcessingStatus
{
    Uploaded,
    Classifying,
    Classified,
    OcrProcessing,
    OcrCompleted,
    Extracting,
    Processed,
    Failed,
    ManualReviewRequired
}

public enum ClassificationStatus
{
    Pending,
    Classified,
    ManualReviewRequired,
    Failed
}

public enum OcrStatus
{
    Pending,
    Processing,
    Completed,
    Skipped,
    Failed
}

public enum ExtractionStatus
{
    Pending,
    Processing,
    Completed,
    Skipped,
    Failed
}

public enum AssignmentMode
{
    None,       // Family/Shared
    Single,     // One person required
    Multiple,   // Multiple persons allowed
    Optional    // Person assignment optional
}
```

## Database Schema (Document-Oriented)

### Firestore (GCP) / DynamoDB (AWS)

**Collection/Table**: `documents`

**Example Document:**

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "createdAt": "2025-12-22T10:00:00Z",
  "updatedAt": "2025-12-22T10:05:00Z",

  "fileName": "rewe_kassenbon_2025-12-22.pdf",
  "fileSizeBytes": 234567,
  "mimeType": "application/pdf",
  "storageUri": "gs://plc-documents/2025/12/550e8400-e29b-41d4-a716-446655440000.pdf",
  "uploadedBy": "user-id-123",

  "categoryId": "everyday-consumption",
  "subcategoryId": "receipts",
  "classificationConfidence": 0.95,
  "classificationStatus": "classified",

  "assignedTo": ["Micha"],
  "tags": ["2025", "rewe", "groceries"],

  "year": 2025,
  "month": 12,
  "quarter": 4,
  "documentDate": "2025-12-22",

  "ocrResult": {
    "fullText": "REWE Markt GmbH...",
    "confidence": 0.92,
    "language": "de",
    "pageCount": 1,
    "processedAt": "2025-12-22T10:02:00Z",
    "provider": "google-document-ai",
    "costCents": 0.15
  },
  "ocrStatus": "completed",

  "extractedData": {
    "type": "receipt",
    "date": "2025-12-22",
    "merchant": "REWE",
    "totalAmount": 42.50,
    "currency": "EUR",
    "vatRates": [
      {"rate": 7, "amount": 1.50},
      {"rate": 19, "amount": 5.80}
    ],
    "paymentMethod": "card",
    "lineItems": [
      {"description": "Milch", "amount": 1.29, "quantity": 2},
      {"description": "Brot", "amount": 2.49, "quantity": 1}
    ],
    "extractedAt": "2025-12-22T10:03:00Z",
    "confidence": 0.88
  },
  "extractionStatus": "completed",

  "processingStatus": "processed",

  "retentionYears": 2,
  "deleteAfter": "2027-12-22T10:00:00Z",

  "customMetadata": {
    "importSource": "manual-upload"
  }
}
```

### Indexes

**Firestore:**
```
- categoryId (ASC), year (DESC)
- assignedTo (ARRAY), year (DESC)
- processingStatus (ASC), createdAt (DESC)
- tags (ARRAY), year (DESC)
- uploadedBy (ASC), createdAt (DESC)
```

**DynamoDB:**
```
Primary Key: id (Partition Key)
GSI1: uploadedBy (PK), createdAt (SK)
GSI2: categoryId (PK), year (SK)
GSI3: processingStatus (PK), createdAt (SK)
```

## Search Index (Elasticsearch / MongoDB)

Für Volltextsuche separater Index:

```json
{
  "documentId": "550e8400-e29b-41d4-a716-446655440000",
  "fullText": "REWE Markt GmbH... [OCR full text]",
  "fileName": "rewe_kassenbon_2025-12-22.pdf",
  "categoryId": "everyday-consumption",
  "subcategoryId": "receipts",
  "assignedTo": ["Micha"],
  "tags": ["2025", "rewe", "groceries"],
  "year": 2025,
  "month": 12,
  "extractedData": {
    "merchant": "REWE",
    "totalAmount": 42.50
  },
  "createdAt": "2025-12-22T10:00:00Z"
}
```

## Repository Pattern

```csharp
namespace PLC.Domain.Repositories;

public interface IDocumentRepository
{
    // Create
    Task<Document> CreateAsync(Document document, CancellationToken ct = default);

    // Read
    Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Document>> GetByUserAsync(Guid userId, int skip, int take, CancellationToken ct = default);
    Task<List<Document>> GetByCategoryAsync(string categoryId, int year, CancellationToken ct = default);
    Task<List<Document>> GetByTagsAsync(List<string> tags, CancellationToken ct = default);

    // Update
    Task UpdateAsync(Document document, CancellationToken ct = default);

    // Delete
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    // Search (delegates to search service)
    Task<SearchResult> SearchAsync(SearchQuery query, CancellationToken ct = default);
}

public interface ISearchService
{
    Task IndexDocumentAsync(Document document, CancellationToken ct = default);
    Task<SearchResult> SearchAsync(SearchQuery query, CancellationToken ct = default);
}
```

## CQRS (Read/Write Separation)

**Write Model**: Firestore/DynamoDB (transactional)

**Read Model**: Elasticsearch/MongoDB (optimized for search)

**Sync**: Event-driven
```
Document updated → Publish DocumentUpdated event
                 → Search Indexer consumes event
                 → Update search index
```

## Nächste Schritte

1. ✅ Data Model definiert
2. ⏳ C# Domain Entities implementieren
3. ⏳ Repository Interfaces definieren
4. ⏳ Firestore/DynamoDB Implementierung
5. ⏳ Elasticsearch Integration (später)
