# 2. Randbedingungen

## 2.1 Technische Randbedingungen

### Backend
- **Programmiersprache Core Domain**: C# .NET 8
- **Programmiersprache Services**: Node.js LTS
- **Architekturstil**: Serverless, Microservices/Nano-Services
- **Cloud Platforms**: Google Cloud (primär), AWS (sekundär), K3s (optional)
- **OCR Service**: Cloud-native (Google Document AI / AWS Textract)

### Frontend
- **Framework**: Svelte
- **Zukünftig**: Windows Explorer Integration

### Datenbank
- **Primär**: Document-orientierte Datenbanken (Firestore, DynamoDB)
- **Sekundär**: Elasticsearch oder MongoDB (CQRS Query-Side)
- **Anforderung**: Repository-Abstraktion für austauschbare Implementierungen

### Infrastructure
- **IaC Tool**: Terraform
- **Observability**: OpenTelemetry (OTEL)
- **Entwicklungsumgebung**: Linux / WSL

## 2.2 Organisatorische Randbedingungen

### Team
- Kleines Team / Solo-Entwicklung
- Agiler Ansatz mit kontinuierlicher Dokumentation

### Prozess
- **Entwicklungsprinzip**: KISS (Keep It Simple, Stupid)
- **Architekturprinzip**: Clean Architecture, Domain-Centric
- **Entscheidungsfindung**: Keine Annahmen ohne Abstimmung
- **Dokumentation**: Arc42 + ADRs von Anfang an

### Budget
- Kostenoptimiert für Privatanwender
- Serverless-First zur Kostenkontrolle
- Keine Enterprise-Skalierung erforderlich

## 2.3 Konventionen

### Code-Konventionen
- **Namensschema**: PLC.* (PaperlessCore)
- **C# Namespaces**: `PLC.Domain`, `PLC.Application`, etc.
- **Service Naming**: Nach Funktion (`PLC.DocumentScanner`)
- **Clean Code**: Konsistente Formatierung via `.editorconfig`

### Dokumentation
- **Sprache**: Deutsch (primär)
- **Format**: Markdown
- **Struktur**: Arc42 Template
- **ADRs**: Für alle wichtigen Architekturentscheidungen

## 2.4 Qualitätsanforderungen (Constraints)

- **Multi-Cloud**: Architektur muss Cloud-Provider-Wechsel ermöglichen
- **Sicherheit**: OWASP Top 10 berücksichtigen
- **Datenschutz**: DSGVO-konform (personenbezogene Daten)
- **Observability**: Vollständige OTEL-Integration ab Start
