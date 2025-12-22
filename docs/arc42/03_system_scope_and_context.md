# 3. Kontextabgrenzung

## 3.1 Fachlicher Kontext

### System-Kontext-Diagramm

```
┌─────────────────────────────────────────────────────────────────────┐
│                         PaperlessCore                               │
│                                                                     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐             │
│  │  PLC.Web     │  │  PLC.Api     │  │  Services    │             │
│  │  (Svelte)    │  │  (C# .NET)   │  │  (Node.js)   │             │
│  └──────────────┘  └──────────────┘  └──────────────┘             │
│         │                 │                  │                      │
└─────────┼─────────────────┼──────────────────┼──────────────────────┘
          │                 │                  │
          │                 │                  │
    ┌─────▼─────┐     ┌─────▼─────┐      ┌────▼────┐
    │   User    │     │  Storage  │      │   OCR   │
    │ (Browser) │     │  (GCS/S3) │      │ Service │
    └───────────┘     └───────────┘      └─────────┘
                           │                   │
                      ┌────▼────┐         ┌────▼────────┐
                      │Database │         │ Document AI │
                      │(Firestore│         │  /Textract  │
                      │/DynamoDB)│         └─────────────┘
                      └─────────┘
                           │
                      ┌────▼────┐
                      │ Search  │
                      │(Later:  │
                      │Elastic) │
                      └─────────┘
```

### Externe Systeme und Akteure

| Akteur/System | Beschreibung | Schnittstelle |
|---------------|--------------|---------------|
| **User (Browser)** | Privatanwender im Haushalt, greift per Web-Browser auf System zu | HTTPS, REST API |
| **Cloud Storage** | Speicherung der PDF/Bild-Dateien | GCS (Google Cloud Storage) / S3 (AWS) |
| **OCR Service** | Text-Extraktion aus Dokumenten | Google Document AI API / AWS Textract API |
| **Database** | Persistierung von Metadaten, OCR-Ergebnissen, extrahierten Daten | Firestore (GCP) / DynamoDB (AWS) |
| **Search Engine** | Volltextsuche (V2) | Elasticsearch / MongoDB Atlas Search |
| **Monitoring** | Observability, Logging, Metrics | Google Cloud Monitoring / CloudWatch + OTEL |

### Fachliche Schnittstellen

#### User → PaperlessCore
- **Upload von Dokumenten**: PDF, JPG, PNG (max 50 MB, max 100 Dateien/Batch)
- **Dokumenten-Verwaltung**: Anzeigen, Suchen, Filtern, Kategorisieren
- **Auswertungen**: Steuerrelevante Reports (z.B. Jahresübersicht Kassenbons)

#### PaperlessCore → Cloud OCR Service
- **Input**: Dokument-URI (gs://... oder s3://...)
- **Output**: Extrahierter Text, Confidence Score, Seitenanzahl
- **Trigger**: Asynchron nach Klassifizierung (nur wenn `ocrEnabled: true`)

#### PaperlessCore → Cloud Storage
- **Input**: Hochgeladene Datei (Multipart Upload)
- **Output**: Storage URI
- **Events**: Object Finalized Event → Trigger Classification

#### PaperlessCore → Database
- **Write**: Document Metadata, OCR-Ergebnisse, Extrahierte Daten
- **Read**: Queries nach Kategorie, Jahr, Person, Tags

## 3.2 Technischer Kontext

### Google Cloud Platform (Primär)

```
┌─────────────────────────────────────────────────────────────┐
│                     Google Cloud Platform                   │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌────────────┐     ┌──────────────┐    ┌──────────────┐  │
│  │ Cloud Run  │     │ Cloud        │    │  Document AI │  │
│  │ (PLC.Api)  │     │ Functions    │    │  (OCR)       │  │
│  └─────┬──────┘     │ (Services)   │    └──────────────┘  │
│        │            └──────┬───────┘                       │
│  ┌─────▼──────────────────▼────────┐                      │
│  │   Cloud Storage (Documents)     │                      │
│  └─────────────────────────────────┘                      │
│        │                                                   │
│  ┌─────▼──────┐         ┌──────────────┐                 │
│  │ Firestore  │         │ Cloud Tasks  │                 │
│  │ (Metadata) │         │ (Queue)      │                 │
│  └────────────┘         └──────────────┘                 │
│        │                                                   │
│  ┌─────▼───────────┐    ┌──────────────┐                 │
│  │ Cloud Monitoring│    │ Cloud Logging│                 │
│  │ (OTEL Metrics)  │    │ (OTEL Logs)  │                 │
│  └─────────────────┘    └──────────────┘                 │
└─────────────────────────────────────────────────────────────┘
```

**Komponenten:**
- **Cloud Run**: PLC.Api (C# ASP.NET Core Container)
- **Cloud Functions**: PLC.DocumentClassifier, PLC.DocumentScanner (Node.js)
- **Cloud Storage**: Dokument-Speicherung
- **Firestore**: NoSQL-Datenbank (Write Model)
- **Document AI**: OCR Service
- **Cloud Tasks**: Async Queue für OCR/Extraction
- **Cloud Monitoring/Logging**: OTEL Integration

### AWS (Sekundär)

```
┌─────────────────────────────────────────────────────────────┐
│                    Amazon Web Services                      │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌────────────┐     ┌──────────────┐    ┌──────────────┐  │
│  │ ECS/Fargate│     │ Lambda       │    │  Textract    │  │
│  │ (PLC.Api)  │     │ Functions    │    │  (OCR)       │  │
│  └─────┬──────┘     │ (Services)   │    └──────────────┘  │
│        │            └──────┬───────┘                       │
│  ┌─────▼──────────────────▼────────┐                      │
│  │   S3 (Documents)                │                      │
│  └─────────────────────────────────┘                      │
│        │                                                   │
│  ┌─────▼──────┐         ┌──────────────┐                 │
│  │ DynamoDB   │         │ SQS          │                 │
│  │ (Metadata) │         │ (Queue)      │                 │
│  └────────────┘         └──────────────┘                 │
│        │                                                   │
│  ┌─────▼───────────┐    ┌──────────────┐                 │
│  │ CloudWatch      │    │ X-Ray        │                 │
│  │ (Metrics/Logs)  │    │ (Tracing)    │                 │
│  └─────────────────┘    └──────────────┘                 │
└─────────────────────────────────────────────────────────────┘
```

### Protokolle und Standards

| Schnittstelle | Protokoll | Format |
|---------------|-----------|--------|
| Frontend ↔ API | HTTPS | REST, JSON |
| API ↔ Storage | HTTPS | Cloud SDK |
| API ↔ Database | HTTPS | Cloud SDK, Document-oriented |
| Service ↔ OCR | HTTPS | REST API, JSON |
| Event Triggers | Cloud-native Events | JSON |
| Monitoring | OTEL | Protobuf / JSON |

## 3.3 Externe Schnittstellen

### 3.3.1 REST API (PLC.Api)

**Base URL**: `https://api.paperlesscore.example.com`

**Authentifizierung**: Bearer Token (JWT) - V2

#### Upload Endpoint
```
POST /api/v1/documents/upload
Content-Type: multipart/form-data

Request Body:
- file: (binary)
- metadata: { "tags": ["2025", "important"] }

Response: 201 Created
{
  "uploadId": "uuid",
  "documentId": "uuid",
  "status": "uploaded"
}
```

#### Get Document
```
GET /api/v1/documents/{id}

Response: 200 OK
{
  "id": "uuid",
  "fileName": "rechnung.pdf",
  "categoryId": "everyday-consumption",
  "processingStatus": "processed",
  ...
}
```

#### Search Documents
```
GET /api/v1/documents/search?q=REWE&year=2025&category=receipts

Response: 200 OK
{
  "results": [...],
  "total": 42,
  "page": 1
}
```

### 3.3.2 Cloud OCR Service API

**Google Document AI**:
- Endpoint: `https://documentai.googleapis.com/v1/projects/{project}/locations/{location}/processors/{processor}:process`
- Auth: Service Account Key
- Input: Base64-encoded document or GCS URI
- Output: Text, Confidence, Entities

**AWS Textract**:
- Endpoint: `https://textract.{region}.amazonaws.com/`
- Auth: AWS SDK (IAM Role)
- Input: S3 URI or Bytes
- Output: Text, Confidence, Blocks

### 3.3.3 Cloud Storage

**GCS**:
- SDK: `@google-cloud/storage` (Node.js), `Google.Cloud.Storage` (C#)
- Events: `google.storage.object.v1.finalized`

**S3**:
- SDK: `@aws-sdk/client-s3` (Node.js), `AWSSDK.S3` (C#)
- Events: `s3:ObjectCreated:*`

### 3.3.4 Database

**Firestore**:
- SDK: `@google-cloud/firestore` (Node.js), `Google.Cloud.Firestore` (C#)
- Query: Document-oriented, NoSQL

**DynamoDB**:
- SDK: `@aws-sdk/client-dynamodb` (Node.js), `AWSSDK.DynamoDBv2` (C#)
- Query: Key-Value + GSI

### 3.3.5 Monitoring (OTEL)

**OpenTelemetry**:
- Traces: Gesamte Processing Pipeline
- Metrics: OCR Kosten, Processing Time, Error Rates
- Logs: Strukturiert, JSON-Format

**Exporter**:
- GCP: Cloud Monitoring/Logging
- AWS: CloudWatch + X-Ray
