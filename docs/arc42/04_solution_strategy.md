# 4. Lösungsstrategie

## 4.1 Technologie-Entscheidungen

### Backend-Strategie

**C# .NET 8 - Domain Core (PLC.Domain, PLC.Application, PLC.Api)**
- Klassische Controller-Service-Repository Trennung
- Clean Architecture mit Domain im Zentrum
- Repository Pattern für Cloud-agnostische Persistierung
- ASP.NET Core Web API (REST)
- Deployment: Cloud Run (GCP) / ECS Fargate (AWS)

**Node.js LTS - Microservices/Serverless (PLC.DocumentClassifier, PLC.DocumentScanner)**
- Serverless Functions für spezialisierte Aufgaben
- Event-driven Processing
- Deployment: Cloud Functions (GCP) / Lambda (AWS)

**Rationale:**
- C# für Business-Logic: Typsicherheit, Performance, Clean Architecture Support
- Node.js für Serverless: Schneller Cold Start, Cloud SDK Support, YAML-Parsing
- Multi-Language ermöglicht "Best Tool for the Job"

### Frontend-Strategie

**Svelte - Web UI (PLC.Web)**
- Reaktive UI ohne Virtual DOM Overhead
- Kleines Bundle (wichtig für schnelle Ladezeiten)
- Einfaches State Management
- TypeScript Support

**Deployment:**
- Static Hosting: Cloud Storage + CDN (GCP) / S3 + CloudFront (AWS)

**Zukunft (V2+):**
- Windows Explorer Integration (OneDrive-ähnlich)
- Mobile App (React Native / Flutter)

### Cloud-Strategie

**Google Cloud Platform (Primär)**
- Cloud Run (Container für PLC.Api)
- Cloud Functions (Node.js Services)
- Cloud Storage (Dokumente)
- Firestore (Metadaten)
- Document AI (OCR)
- Cloud Tasks (Queuing)

**AWS (Sekundär)**
- ECS Fargate (Container für PLC.Api)
- Lambda (Node.js Services)
- S3 (Dokumente)
- DynamoDB (Metadaten)
- Textract (OCR)
- SQS (Queuing)

**Multi-Cloud Abstraktion:**
- Repository Pattern für Datenzugriff
- IOcrService Interface für OCR
- IStorageService Interface für Object Storage
- Terraform Module pro Cloud-Provider

### Persistierung

**Write Model (Transaktional):**
- Firestore (GCP) / DynamoDB (AWS)
- Document-oriented für flexible Schema
- Keine Joins erforderlich

**Read Model (Search - V2):**
- Elasticsearch / MongoDB Atlas Search
- Optimiert für Volltextsuche
- CQRS Pattern

### OCR & ML

**Cloud-native Services:**
- Google Document AI (GCP)
- AWS Textract (AWS)

**Rationale:**
- Höhere Qualität als Self-Hosted (Tesseract)
- Pay-per-Use (kosteneffizient)
- Deutsche Sprache out-of-the-box

### Infrastructure as Code

**Terraform:**
- Multi-Cloud Support
- Versionierbar, reproduzierbar
- Modular: `/infrastructure/{gcp|aws}/modules`
- Environment-basiert: `/infrastructure/{gcp|aws}/envs/default`

### Observability

**OpenTelemetry (OTEL):**
- Traces: End-to-End Pipeline
- Metrics: Kosten, Performance, Errors
- Logs: Strukturiert, JSON

**Backend:**
- GCP: Cloud Monitoring + Cloud Logging
- AWS: CloudWatch + X-Ray

## 4.2 Top-Level Zerlegung

### Processing Pipeline (5 Stages)

```
Upload → Store → Classify → OCR → Extract
(Sync)   (Sync)  (Async)    (Async) (Async)
```

#### 1. Upload Stage
**Komponente:** `PLC.Web` (Frontend) + `PLC.Api` (Backend)

- User wählt Datei(en)
- Validierung (Format, Größe, Batch)
- Upload zu Cloud Storage
- Metadaten in DB schreiben

#### 2. Classification Stage
**Komponente:** `PLC.DocumentClassifier` (Node.js Function)

- Trigger: Storage Event (Object Created)
- Klassifizierung: Rule-based (V1) / ML (V2)
- Kategorie + Subcategory aus `document-structure.yaml`
- Confidence Check → Manual Review bei Low-Confidence

#### 3. OCR Stage (Conditional)
**Komponente:** `PLC.DocumentScanner` (Node.js Function)

- Trigger: Queue Message (nur wenn `ocrEnabled: true`)
- Cloud OCR API aufrufen
- Full Text extrahieren
- Ergebnis in DB speichern

#### 4. Extraction Stage (Conditional)
**Komponente:** `PLC.Functions.*` oder integriert in Classifier

- Trigger: Queue Message (nur wenn `dataExtraction: true`)
- Strukturierte Daten extrahieren (z.B. Datum, Betrag, Händler)
- Regex/Pattern (V1) / ML-NER (V2)
- In DB speichern

#### 5. Indexing (V2)
**Komponente:** Search Indexer

- Trigger: Document Updated Event
- Elasticsearch/MongoDB Index aktualisieren

### Backend-Komponenten

```
┌─────────────────────────────────────────────────┐
│              PLC.Api (C# .NET 8)                │
├─────────────────────────────────────────────────┤
│  Controllers                                    │
│  ├─ DocumentsController                         │
│  ├─ SearchController                            │
│  └─ ReportsController                           │
├─────────────────────────────────────────────────┤
│  PLC.Application (Services)                     │
│  ├─ DocumentService                             │
│  ├─ ClassificationService                       │
│  └─ ReportingService                            │
├─────────────────────────────────────────────────┤
│  PLC.Domain (Entities, Value Objects)           │
│  ├─ Document                                    │
│  ├─ OcrResult                                   │
│  └─ ExtractedData                               │
├─────────────────────────────────────────────────┤
│  PLC.Infrastructure (External Services)         │
│  ├─ GoogleCloudStorageService                   │
│  ├─ GoogleDocumentAiOcrService                  │
│  └─ FirestoreDocumentRepository                 │
├─────────────────────────────────────────────────┤
│  PLC.Persistence (Data Access)                  │
│  ├─ IDocumentRepository                         │
│  └─ Firestore/DynamoDB Implementations          │
└─────────────────────────────────────────────────┘
```

### Node.js Services

```
PLC.DocumentClassifier (Cloud Function)
├─ Triggered by: Storage Event
├─ Reads: document-structure.yaml
├─ Classifies: Document Type
└─ Publishes: Classification Result + OCR Task

PLC.DocumentScanner (Cloud Function)
├─ Triggered by: Queue Message
├─ Calls: Document AI / Textract API
├─ Stores: OCR Result
└─ Publishes: Extraction Task
```

### Frontend

```
PLC.Web (Svelte)
├─ /upload       - Dokumente hochladen
├─ /documents    - Liste aller Dokumente
├─ /search       - Volltextsuche
├─ /reports      - Auswertungen (Steuer, etc.)
└─ /settings     - Einstellungen
```

## 4.3 Architekturmuster

### Clean Architecture

**Dependency Flow:**
```
PLC.Api → PLC.Application → PLC.Domain
             ↓
     PLC.Infrastructure
             ↓
      PLC.Persistence
```

**Prinzipien:**
- Domain kennt keine Infrastructure
- Dependency Inversion via Interfaces
- Repository Pattern
- Use Cases in Application Layer

### Event-Driven Architecture

**Events:**
1. `DocumentUploaded` → Trigger Classification
2. `DocumentClassified` → Trigger OCR (if enabled)
3. `OcrCompleted` → Trigger Extraction (if enabled)
4. `DocumentProcessed` → Update Search Index (V2)

**Implementation:**
- GCP: Cloud Storage Events + Pub/Sub + Cloud Tasks
- AWS: S3 Events + EventBridge + SQS

### CQRS (V2)

**Write Model:**
- Firestore/DynamoDB
- Transaktional, normalisiert

**Read Model:**
- Elasticsearch / MongoDB
- Denormalisiert, optimiert für Queries
- Eventual Consistency

### Repository Pattern

**Interface (Domain Layer):**
```csharp
public interface IDocumentRepository
{
    Task<Document> CreateAsync(Document doc);
    Task<Document?> GetByIdAsync(Guid id);
    Task UpdateAsync(Document doc);
}
```

**Implementation (Infrastructure Layer):**
- `FirestoreDocumentRepository`
- `DynamoDbDocumentRepository`

→ Wechsel zwischen Cloud-Providern ohne Domain-Änderung

## 4.4 Qualitätsziele erreichen

| Qualitätsziel | Lösungsansatz | Umsetzung |
|---------------|---------------|-----------|
| **Einfachheit (KISS)** | Keine Over-Engineering | - Nur Features die benötigt werden<br>- YAML-Config statt komplexe DB-Schema<br>- Cloud-Managed Services (kein K8s) |
| **Kosteneffizienz** | Serverless + Selective Processing | - Pay-per-Use (Cloud Functions)<br>- OCR nur wenn nötig (`ocrEnabled`)<br>- Batch Processing nachts<br>- Ziel: < 10 EUR/Monat |
| **Multi-Cloud** | Abstraktionen | - Repository Pattern<br>- IOcrService Interface<br>- Terraform Module<br>- Provider-agnostische Domain |
| **Wartbarkeit** | Clean Architecture | - Domain-Centric<br>- Klare Schichten<br>- Umfassende Dokumentation (Arc42 + ADRs)<br>- Typsicherheit (C#, TypeScript) |
| **Observability** | OTEL von Anfang an | - Traces über gesamte Pipeline<br>- Metrics: Kosten, Performance<br>- Strukturierte Logs<br>- Cost Tracking |

## 4.5 Entscheidungsübersicht

Alle wichtigen Architekturentscheidungen sind als ADRs dokumentiert:

- **[ADR-001](../adr/adr-001-document-categorization-strategy.md)**: Kategorisierung & Personenzuordnung
- **[ADR-002](../adr/adr-002-upload-processing-pipeline.md)**: Upload & Processing Pipeline
- **[ADR-003](../adr/adr-003-ocr-strategy.md)**: OCR Strategie (Async, Type-Dependent)

## 4.6 Risiken & Trade-offs

### Vendor Lock-in (Cloud)
**Risk:** Abhängigkeit von GCP/AWS spezifischen Services

**Mitigation:**
- Multi-Cloud Abstraktion (Repository, IOcrService)
- Terraform IaC (reproduzierbar)
- Sekundärer Cloud-Provider validiert Abstraktion

### Eventual Consistency
**Risk:** Dokument nicht sofort durchsuchbar

**Trade-off:** Akzeptiert für bessere Skalierung & Kosten

**Mitigation:**
- Status-Tracking
- User-Feedback ("Wird verarbeitet...")

### Multi-Language Complexity
**Risk:** C# + Node.js = mehr Komplexität

**Trade-off:** Best Tool for the Job

**Mitigation:**
- Klare Grenzen (C# = Core, Node.js = Serverless)
- Shared Config (YAML)
- Konsistentes Logging (OTEL)
