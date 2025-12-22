# ADR-002: Upload and Processing Pipeline

## Status

Accepted

## Kontext

PaperlessCore muss Dokumente vom Benutzer entgegennehmen und durch eine mehrstufige Verarbeitungspipeline leiten. Die Herausforderungen:

### Anforderungen

**Funktional:**
- Einzeldokument-Upload (1 PDF)
- Bulk-Upload (mehrere PDFs gleichzeitig)
- Unterstützte Formate: PDF, JPG, JPEG, PNG
- Max. Dateigröße: 50 MB pro Datei
- Max. Batch-Größe: 100 Dateien

**Non-Funktional:**
- Kosteneffizient (< 10 EUR/Monat für durchschnittliche Nutzung)
- Serverless-optimiert (Pay-per-Use)
- Asynchrone Verarbeitung (keine Blockierung des Users)
- Robust (Fehlerbehandlung, Retries)
- Nachvollziehbar (jeder Schritt geloggt via OTEL)

### V1-Scope (MVP)

**Start einfach:**
- Svelte Web-Frontend mit Upload-Formular
- Keine Mobile App, kein Scanner-Integration
- Keine Ordner-Überwachung

**Später (V2+):**
- Ordner-Überwachung (wie iCloud Sync)
- Mobile App (Foto-Upload)
- E-Mail-Integration (Rechnungen forwarden)
- Windows Explorer Integration

### Bestehendes Setup

- 3.195 PDFs (3,3 GB) müssen später migriert werden
- Migration: Schrittweise, neue Dokumente zuerst
- Fehler-getrieben: Issues in GitHub

## Entscheidung

### Processing Pipeline (5 Stages)

```
┌──────────┐    ┌──────────┐    ┌──────────────┐    ┌────────────┐    ┌────────────┐
│  UPLOAD  │───>│  STORE   │───>│  CLASSIFY    │───>│    OCR     │───>│  EXTRACT   │
└──────────┘    └──────────┘    └──────────────┘    └────────────┘    └────────────┘
   Sync           Sync            Async (Queue)       Async (Queue)     Async (Queue)
```

### Stage 1: UPLOAD (Synchronous)

**Komponente:** `PLC.Web` (Svelte Frontend) + `PLC.Api` (C# Backend)

**Flow:**
1. User wählt Datei(en) im Frontend
2. POST zu `/api/documents/upload`
3. Validierung:
   - Dateiformat (PDF, JPG, JPEG, PNG)
   - Dateigröße (max 50 MB)
   - Batch-Size (max 100 Dateien)
4. Response: `uploadId` und `documentIds[]`

**Technologie:**
- Frontend: Svelte mit File Input (Drag & Drop support)
- Backend: ASP.NET Core Web API
- Multipart/form-data Upload

### Stage 2: STORE (Synchronous)

**Komponente:** `PLC.Api` + Cloud Storage

**Flow:**
1. Datei in Object Storage speichern
   - GCP: Google Cloud Storage Bucket
   - AWS: S3 Bucket
2. Metadata in Datenbank schreiben:
   ```json
   {
     "documentId": "uuid",
     "fileName": "rechnung.pdf",
     "uploadedAt": "2025-12-22T10:00:00Z",
     "uploadedBy": "userId",
     "fileSize": 1234567,
     "mimeType": "application/pdf",
     "storageUri": "gs://plc-documents/2025/12/uuid.pdf",
     "status": "uploaded",
     "processingStatus": {
       "classification": "pending",
       "ocr": "pending",
       "extraction": "pending"
     }
   }
   ```
3. **Trigger Classification** via Event/Message

**Technologie:**
- GCP: Cloud Storage + Firestore
- AWS: S3 + DynamoDB
- Repository Pattern (abstrahiert in `PLC.Persistence`)

### Stage 3: CLASSIFICATION (Asynchronous)

**Komponente:** `PLC.DocumentClassifier` (Node.js Serverless Function)

**Trigger:**
- GCP: Cloud Storage Object Finalized Event → Cloud Function
- AWS: S3 Event Notification → Lambda

**Flow:**
1. Event empfangen (neue Datei in Storage)
2. Document Metadata aus DB laden
3. Klassifizierung durchführen:
   - **V1**: Rule-based (z.B. Dateiname-Patterns, simple ML)
   - **V2**: ML-Model (z.B. Google AutoML, AWS SageMaker)
4. Kategorie + Subkategorie bestimmen (aus `document-structure.yaml`)
5. Confidence Score berechnen
6. Wenn Confidence < Threshold (0.85):
   - Status: `manual-review-required`
7. Sonst:
   - Kategorie in DB speichern
   - Status: `classified`
   - **Trigger OCR** (wenn `ocrEnabled: true` für Kategorie)

**Technologie:**
- Node.js LTS (Serverless Function)
- YAML Parser für Config
- Event-driven

### Stage 4: OCR (Asynchronous, Conditional)

**Komponente:** `PLC.DocumentScanner` (Node.js Serverless Function)

**Trigger:**
- Nur wenn `ocrEnabled: true` für klassifizierte Kategorie
- Message Queue / Event

**Flow:**
1. Prüfe: OCR erforderlich? (aus YAML Config)
2. Wenn nein: Skip, Status: `ocr-skipped`
3. Wenn ja:
   - Datei aus Storage laden
   - Cloud OCR Service aufrufen:
     - **GCP**: Document AI API
     - **AWS**: Textract
   - OCR-Ergebnis (Full Text) speichern
   - Status: `ocr-completed`
   - **Trigger Extraction** (wenn `dataExtraction: true`)

**Kostenoptimierung:**
- Batch Processing: Nächtliche Verarbeitung bevorzugen (22:00-06:00)
- Caching: OCR-Ergebnisse cachen
- Retry-Logic: Max 3 Versuche

**Technologie:**
- Node.js LTS
- Google Document AI / AWS Textract SDK
- Queue-basiert (z.B. Cloud Tasks, SQS)

### Stage 5: EXTRACTION (Asynchronous, Conditional)

**Komponente:** `PLC.Functions.*` oder `PLC.DocumentClassifier`

**Trigger:**
- Nur wenn `dataExtraction: true` für Kategorie
- Nach erfolgreichem OCR

**Flow:**
1. Prüfe: Extraktion erforderlich? (aus YAML Config)
2. Wenn nein: Skip, Status: `extraction-skipped`
3. Wenn ja:
   - OCR-Text laden
   - Extraction Fields aus Config laden (z.B. `date, merchant, totalAmount` für Receipts)
   - Extraktion durchführen:
     - **V1**: Regex/Pattern-Matching
     - **V2**: ML-Model (NER - Named Entity Recognition)
   - Strukturierte Daten in DB speichern:
     ```json
     {
       "documentId": "uuid",
       "extractedData": {
         "date": "2025-12-22",
         "merchant": "REWE",
         "totalAmount": 42.50,
         "vatRates": [{"rate": 7, "amount": 1.50}, {"rate": 19, "amount": 5.00}],
         "paymentMethod": "card"
       }
     }
     ```
   - Status: `extraction-completed`
4. Final Status: `processed`

**Technologie:**
- Node.js LTS
- Regex + Pattern Libraries
- Später: ML (Google AutoML, spaCy NER)

### Error Handling & Retries

**Strategie:**
- Jeder Stage kann fehlschlagen
- Max 3 Retry-Versuche pro Stage
- Exponential Backoff
- Nach 3 Fehlversuchen: Status `failed`, Reason in DB
- User-Notification bei kritischen Fehlern

**Monitoring:**
- OTEL Traces über gesamte Pipeline
- Metrics: Processing Time pro Stage
- Alerts bei hoher Fehlerrate

### Status-Modell

```
uploaded → classifying → classified → ocr-processing → ocr-completed → extracting → processed
                ↓            ↓              ↓                ↓               ↓
           manual-review  failed        ocr-failed      extraction-failed
```

## Konsequenzen

### Positiv

- Async & Non-Blocking: User wartet nicht auf Verarbeitung
- Serverless-optimiert: Pay-per-Use, Auto-Scaling
- Event-Driven: Lose Kopplung, einfach erweiterbar
- Kosteneffizient: Nur OCR wenn nötig, Batch-Processing nachts, kein Always-On Server
- Robust: Retries, Error-Handling, Status-Tracking
- Nachvollziehbar: OTEL über gesamte Pipeline
- Flexibel: Neue Stages einfach hinzufügbar
- KISS: Simple Pipeline, kein Over-Engineering

### Negativ

- Eventual Consistency: Dokument nicht sofort durchsuchbar
- Komplexität: 5 Stages, mehrere Services
- Debugging: Async/Distributed schwerer zu debuggen (daher OTEL wichtig)
- Cold Starts: Serverless Functions haben Latenz bei erstem Request

### Risiken

Cloud Vendor Lock-in: GCP/AWS spezifische Services

Mitigation: Repository Pattern abstrahiert Storage, OCR-Service Interface abstrahiert Document AI/Textract, IaC mit Terraform (multi-cloud)

Kosten-Explosion: Bei vielen Dokumenten

Mitigation: Batch Processing, Cost Alerts, Rate Limiting, Dokumenttyp-basiertes OCR

Queue-Overflow: Bei vielen gleichzeitigen Uploads

Mitigation: Queue-Limits, Backpressure, User-Feedback bei hoher Last

## Alternativen

### Alternative A: Synchrone Verarbeitung

Alle Stages synchron im Upload-Request.

**Abgelehnt weil:**
- User wartet mehrere Sekunden (schlechte UX)
- Timeout-Risiko
- Nicht skalierbar
- Serverless ungeeignet (Cold Starts)

### Alternative B: Single Monolith Service

Ein Service für alle Stages.

**Abgelehnt weil:**
- Kein Serverless (Always-On Server = teuer)
- Schlechte Skalierung (OCR ist CPU-intensiv)
- Schwerer zu warten

### Alternative C: Microservices mit K8s

Jede Stage als Microservice in Kubernetes.

**Abgelehnt weil:**
- V1 braucht keine Netflix-Skalierung
- K8s = Overhead + Kosten
- Komplexität zu hoch für MVP
- Widerspricht "Serverless First"

### Alternative D: OCR immer synchron

OCR sofort beim Upload (auch wenn später nicht benötigt).

**Abgelehnt weil:**
- Unnötige Kosten (Lohnzettel brauchen kein OCR in V1)
- Langsamer für User
- Widerspricht dokumenttyp-basierter Strategie

## Nächste Schritte

1. Pipeline definiert
2. ADR-003: OCR Strategy Details
3. Domain Model (Document Entity)
4. API Design (OpenAPI Spec)
5. Proof of Concept: Simple Upload + Storage

## Referenzen

- ADR-001: Document Categorization Strategy
- ADR-003: OCR Strategy (folgt)
- `src/config/document-structure.yaml` - Processing Regeln
- GitHub Repo: https://github.com/medoni/paperless-core
