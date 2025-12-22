# ADR-003: OCR Strategy (Async, Type-Dependent)

## Status

Accepted

## Kontext

PaperlessCore benötigt OCR (Optical Character Recognition), um Text aus gescannten PDFs und Bildern zu extrahieren. Die Herausforderungen:

### Anforderungen

**Funktional:**
- Text aus PDFs extrahieren (gescannte Dokumente)
- Text aus Bildern extrahieren (JPG, PNG)
- Deutsche Sprache erkennen
- Qualität: Hohe Erkennungsrate bei Haushaltsdokumenten (Rechnungen, Kassenbons)

**Non-Funktional:**
- **Kosteneffizient**: Budget < 10 EUR/Monat (für normale Nutzung)
- **Nicht alles OCRen**: Manche Dokumente brauchen kein OCR (z.B. Lohnzettel in V1)
- **Asynchron**: Keine Blockierung des Users
- **Skalierbar**: Batch-Processing für Migration (3.195 PDFs)

### Dokumenttyp-abhängige OCR-Anforderungen

Aus `document-structure.yaml` und Diskussion:

**OCR erforderlich:**
- Kassenbons (`receipts`): Ja → für Datenextraktion
- Rechnungen (`invoices`): Ja → für Volltext + Extraktion
- Bauunterlagen (`renovations`): Ja → für Volltextsuche
- Steuerdokumente (`tax-*`): Ja → für Volltext
- Eigentumsdokumente (`property-ownership`): Ja → für Volltext

**OCR NICHT erforderlich (V1):**
- Lohnzettel (`salary-statement`): Nein → nur Klassifizierung
- Rezepte (`prescriptions`): Nein → kurze Aufbewahrungsfrist
- Hobbys (`hobbies`): Nein → unwichtig

### Kosten-Überlegungen

**Google Document AI Pricing (Stand 2025):**
- OCR: ~$1,50 per 1.000 pages
- Bei 50 neuen Dokumenten/Monat (Ø 2 Seiten): ~$0,15/Monat

**AWS Textract Pricing:**
- OCR: ~$1,50 per 1.000 pages (ähnlich)

**Einmalige Migration:**
- ~3.500 PDFs (Ø 2 Seiten) ≈ 7.000 Seiten
- Kosten: ~$10 (einmalig, nicht im laufenden Budget)

### Cloud Provider Anforderungen

- **GCP primär**: Google Cloud Document AI
- **AWS sekundär**: AWS Textract
- Multi-Cloud: Interface abstrahieren

## Entscheidung

### 1. Cloud-native OCR Services

**Wir verwenden Cloud-Provider OCR Services:**

- **Google Cloud**: Document AI (Primary)
- **AWS**: Textract (Secondary)

**NICHT selbst hosten** (Tesseract, etc.)

**Begründung:**
- Höhere Qualität (ML-optimiert)
- Serverless (Pay-per-Use)
- Keine Infrastruktur-Wartung
- Deutsche Sprache out-of-the-box
- Kosteneffizient bei unserer Nutzung

### 2. Dokumenttyp-abhängige OCR (Selective OCR)

**OCR nur wenn `ocrEnabled: true` in Config:**

```yaml
subcategories:
  - id: receipts
    ocrEnabled: true    # ✅ OCR durchführen

  - id: salary-statement
    ocrEnabled: false   # ❌ OCR überspringen (V1)
```

**Vorteile:**
- Kostenersparnis (nur nötiges OCR)
- Schnellere Verarbeitung
- Flexibel konfigurierbar

**Flow:**
```
Document classified → Check YAML config
  ├─ ocrEnabled: true  → Trigger OCR Service
  └─ ocrEnabled: false → Skip, Status: "ocr-skipped"
```

### 3. Asynchrone Verarbeitung via Queue

**OCR läuft asynchron nach Classification:**

**Architektur:**
```
Classification Service
  └─> Push to OCR Queue (if ocrEnabled)
       └─> OCR Worker (Serverless Function)
            └─> Call Cloud OCR API
                 └─> Store result in DB
                      └─> Trigger Extraction (if needed)
```

**Queue-Technologie:**
- **GCP**: Cloud Tasks oder Pub/Sub
- **AWS**: SQS

**Vorteile:**
- Non-blocking für User
- Retry-Logic eingebaut
- Backpressure-Handling
- Rate-Limiting möglich

### 4. Batch Processing für Kostenoptimierung

**Konfiguration in `document-structure.yaml`:**

```yaml
costOptimization:
  ocrBatchProcessing: true
  preferredProcessingHours: "22:00-06:00"  # Nachts
  cacheOcrResults: true
```

**Strategie:**
- **Echtzeit**: Wichtige Dokumente (z.B. `important` Tag)
- **Batch**: Normale Dokumente in nächtlichen Batches
- **User-Choice**: "Jetzt verarbeiten" Button für dringende Fälle

**Implementierung:**
- Queue mit Priority (High/Normal)
- Scheduler für Batch-Jobs (Cloud Scheduler / EventBridge)

### 5. OCR Result Storage

**Speicherung:**

```json
{
  "documentId": "uuid",
  "ocrResult": {
    "fullText": "Extracted text here...",
    "confidence": 0.95,
    "language": "de",
    "pageCount": 2,
    "processedAt": "2025-12-22T10:00:00Z",
    "provider": "google-document-ai",
    "costCents": 0.3
  }
}
```

**Caching:**
- OCR-Ergebnis cachen (nicht neu OCRen bei wiederholtem Zugriff)
- Invalidierung: Nur bei neuem Upload

### 6. Multi-Cloud Abstraction

**Interface:** `IOcrService`

```csharp
namespace PLC.Infrastructure.Ocr;

public interface IOcrService
{
    Task<OcrResult> ExtractTextAsync(
        string storageUri,
        string language = "de",
        CancellationToken cancellationToken = default
    );
}

public record OcrResult(
    string FullText,
    double Confidence,
    string Language,
    int PageCount,
    decimal CostCents
);
```

**Implementierungen:**
- `GoogleDocumentAiOcrService`
- `AwsTextractOcrService`

**Dependency Injection:**
```csharp
// GCP
services.AddSingleton<IOcrService, GoogleDocumentAiOcrService>();

// AWS
services.AddSingleton<IOcrService, AwsTextractOcrService>();
```

### 7. Error Handling & Retries

**Retry-Strategie:**
- Max 3 Versuche
- Exponential Backoff (1s, 2s, 4s)
- Bei Fehler: Status `ocr-failed`, Reason in DB
- User-Benachrichtigung bei kritischen Fehlern

**Fehlerarten:**
- API Rate Limit → Retry mit Backoff
- Invalid File → Permanent Failure, kein Retry
- Network Error → Retry
- OCR Confidence < Threshold → Markieren für Manual Review

### 8. Monitoring & Cost Tracking

**OTEL Metrics:**
- `ocr.requests.total` (Counter)
- `ocr.cost.total` (Counter in Cents)
- `ocr.duration.seconds` (Histogram)
- `ocr.confidence.score` (Histogram)

**Cost Alerts:**
- Alarm bei > 10 EUR/Monat
- Dashboard für Cost-Tracking

**Quality Metrics:**
- Durchschnittliche Confidence
- Fehlerrate
- Processing Time

## Konsequenzen

### Positiv

- Kosteneffizient: Nur OCR wo nötig, Batch-Processing nachts, < 10 EUR/Monat
- Hohe Qualität: Cloud-Services besser als Tesseract
- Serverless: Pay-per-Use, kein Always-On Server
- Flexibel: Per Config steuerbar (`ocrEnabled`)
- Multi-Cloud Ready: Abstraction via Interface
- Async: Keine Blockierung des Users
- Nachvollziehbar: OTEL Metrics + Cost Tracking

### Negativ

- Vendor Lock-in: Abhängig von GCP/AWS OCR-Services (Mitigation: Interface-Abstraktion)
- Eventual Consistency: Text nicht sofort verfügbar (Mitigation: Status-Tracking, User-Feedback)
- Network-abhängig: API-Calls können fehlschlagen (Mitigation: Retry-Logic, Error-Handling)
- Kosten bei Fehler: Falsche Klassifizierung → unnötiges OCR (Mitigation: Confidence-Threshold 0.85)

### Risiken

API-Kosten steigen: Bei vielen Dokumenten

Mitigation: Cost Alerts, Rate Limiting, User-Quota (später)

OCR-Qualität unzureichend: Handschrift, schlechte Scans

Mitigation: Confidence-Tracking, Manual Review bei Low-Confidence, User-Feedback-Loop

API-Änderungen: Provider ändert API oder Pricing

Mitigation: Abstraction Layer, Version Pinning, Cost-Monitoring

## Alternativen

### Alternative A: Tesseract (Self-Hosted)

Open-Source OCR, selbst betrieben.

**Abgelehnt weil:**
- Schlechtere Qualität als Cloud-Services
- Infrastructure-Overhead (Server, Wartung)
- Nicht Serverless (Always-On = teuer)
- Deutsche Sprache weniger gut
- Mehr Komplexität

### Alternative B: Immer OCR (alle Dokumente)

Jedes Dokument OCRen, unabhängig vom Typ.

**Abgelehnt weil:**
- Unnötige Kosten (Lohnzettel brauchen kein OCR in V1)
- Langsamer
- Widerspricht KISS

### Alternative C: OCR on-demand (beim ersten Zugriff)

OCR erst durchführen, wenn User danach sucht.

**Abgelehnt weil:**
- Schlechte UX (User wartet beim Suchen)
- Schwer zu cachen
- Komplexer zu implementieren
- Batch-Processing nicht möglich

### Alternative D: Hybrid (Cloud + Self-Hosted)

Cloud für wichtige Docs, Tesseract für unwichtige.

**Abgelehnt weil:**
- Zu komplex für V1
- Inkonsistente Qualität
- Wartungs-Overhead
- KISS-Prinzip verletzt

## Nächste Schritte

1. OCR-Strategie definiert
2. `IOcrService` Interface implementieren (C#)
3. Google Document AI Integration (POC)
4. OCR Worker Function (Node.js)
5. Queue-Setup (Cloud Tasks / SQS)
6. Cost-Tracking + OTEL Metrics

## Referenzen

- ADR-001: Document Categorization Strategy
- ADR-002: Upload and Processing Pipeline
- `src/config/document-structure.yaml` - `ocrEnabled` Config
- Google Document AI: https://cloud.google.com/document-ai
- AWS Textract: https://aws.amazon.com/textract/
