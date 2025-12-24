# PaperlessCore - Backend Implementation

## Architecture

Clean Architecture mit Domain-Centric Design:

```
PLC.Api/                    # REST API Layer (Controllers, DTOs)
PLC.Application/            # Application Layer (Commands, Handlers)
PLC.Infrastructure/         # Infrastructure Layer (GCP Services)
PLC.Domain/                 # Domain Layer (Entities, Events)
PLC.Persistence/            # (Future: Database migrations, seeds)
PLC.Shared/                 # (Future: Shared utilities)
```

## Document Upload Flow

### 1. Upload (Synchronous)
```
POST /api/v1/documents
 ↓
UploadDocumentHandler
 ├─ Validate (format, size, batch)
 ├─ Upload to GCS (IDocumentStorageService)
 ├─ Save to Firestore (IDocumentRepository)
 ├─ Publish DocumentChangedEvent (IEventPublisher)
 └─ Return UploadResponse
```

### 2. Processing (Asynchronous - Event-Driven)
```
DocumentChangedEvent
 ↓
Pub/Sub Topic: "document-changed"
 ↓
Workers (idempotent):
 ├─ Classifier: Checks NeedsClassification()
 ├─ OCR Worker: Checks NeedsOcr()
 └─ Extractor: Checks NeedsExtraction()
```

## Document Status Model (Simplified)

### Status Enum
- `Uploading` - Upload in progress
- `Processing` - Being processed (classification, OCR, extraction)
- `Finished` - All processing complete
- `Deleted` - Soft-deleted

### Worker Logic (Idempotent)
Each worker:
1. Receives `DocumentChangedEvent`
2. Loads document from Firestore
3. Checks if work is needed:
   - Classifier: `document.NeedsClassification()` → `category == null`
   - OCR: `document.NeedsOcr(ocrEnabled)` → `category != null && ocrEnabled && ocrText == null`
   - Extractor: `document.NeedsExtraction(extractionEnabled)` → `category != null && extractionEnabled && extractedData == null`
4. If no work needed → Return (idempotent!)
5. If work needed → Process and update document
6. Publish `DocumentChangedEvent.Updated()` for next worker

### Eventual Consistency
- Document status transitions: `Uploading` → `Processing` → `Finished`
- Each worker updates Firestore independently
- Frontend polls `GET /api/v1/documents/{id}` for status updates

## Configuration

### appsettings.json
```json
{
  "GoogleCloud": {
    "ProjectId": "paperless-core-dev-447208",
    "StorageBucket": "paperless-core-dev-447208-documents",
    "FirestoreDatabase": "(default)",
    "FirestoreCollection": "plc-documents",
    "PubSubTopic": "document-changed"
  }
}
```

### Environment Variables
- `GOOGLE_APPLICATION_CREDENTIALS` - Path to service account JSON (local dev)
- `ASPNETCORE_ENVIRONMENT` - Development/Production

## Running Locally

### Prerequisites
1. .NET 8 SDK
2. Google Cloud credentials (service account JSON)

### Setup
```bash
cd src/backend/PLC.Api

# Set credentials
export GOOGLE_APPLICATION_CREDENTIALS="/path/to/service-account.json"

# Restore packages
dotnet restore

# Run
dotnet run
```

API available at: `http://localhost:8080`
Swagger UI: `http://localhost:8080/swagger`

## Testing Upload

### Using cURL
```bash
curl -X POST http://localhost:8080/api/v1/documents \
  -F "files=@/path/to/document.pdf" \
  -F "tags=2025,test" \
  -F "assignedTo=Micha"
```

### Using Swagger UI
1. Navigate to `http://localhost:8080/swagger`
2. POST /api/v1/documents
3. Try it out
4. Upload file
5. Execute

### Check Status
```bash
curl http://localhost:8080/api/v1/documents/{documentId}
```

## Next Steps

### Backend (Current Implementation)
- ✅ Domain Layer: Document Entity + Events
- ✅ Infrastructure: GCS, Firestore, Pub/Sub
- ✅ Application: Upload Handler
- ✅ API: POST /api/v1/documents, GET /api/v1/documents/{id}
- ✅ OpenTofu: Pub/Sub Topic + Subscription

### Node.js Classifier (Next Session)
- 📋 Cloud Function setup
- 📋 Rule-based classification logic
- 📋 Slug generation (fallback: date + title)
- 📋 Firestore update
- 📋 OpenTofu deployment

### Future Workers
- 📋 OCR Worker (Google Document AI)
- 📋 Extraction Worker (Pattern matching / ML)

## Dependencies

### NuGet Packages
- `Google.Cloud.Storage.V1` - Cloud Storage client
- `Google.Cloud.Firestore` - Firestore client
- `Google.Cloud.PubSub.V1` - Pub/Sub client
- `Microsoft.AspNetCore.OpenApi` - OpenAPI support
- `Swashbuckle.AspNetCore` - Swagger UI

## Architecture Decisions

See [ADR-002: Upload and Processing Pipeline](../../docs/adr/adr-002-upload-processing-pipeline.md)

Key decisions:
- **Event-Driven Architecture** - Single event, multiple workers
- **Idempotent Workers** - Workers check if work is needed
- **Eventual Consistency** - Status updates via polling (V1)
- **Simplified Status Model** - 4 states instead of complex state machine
