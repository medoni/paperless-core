# PaperlessCore (PLC)

> Intelligent Document Management for Private Households

## Vision

PaperlessCore ist eine Cloud-native Anwendung zur automatischen Verarbeitung, Klassifizierung und Verwaltung von Haushaltsdokumenten. Die Lösung ermöglicht es Privatanwendern, ihr papierloses Büro effizient zu organisieren - von Kassenzetteln über Versicherungspolicen bis hin zu Bauunterlagen.

### Kernfunktionen

- **Automatische Dokumentenverarbeitung**: Upload und OCR-Scan von PDF-Dokumenten
- **Intelligente Klassifizierung**: KI-gestützte Erkennung von Dokumenttypen (Rechnung, Kassenzettel, Versicherung, etc.)
- **Strukturierte Datenextraktion**: Automatische Erfassung relevanter Informationen (Datum, Beträge, MwSt, Positionen)
- **Volltextsuche**: Schnelles Auffinden von Dokumenten und Inhalten
- **Steuerrelevante Auswertungen**: Jahresübersichten für Einkommensteuererklärung

## Technologie-Stack

### Backend
- **Core Domain Logic**: C# .NET 8 (Clean Architecture)
- **Microservices/Nano-Services**: Node.js LTS (Serverless Functions)
- **Cloud Platforms**: Google Cloud (primär), AWS (sekundär), K3s (self-hosted)
- **Architecture**: Domain-centric, CQRS-ready

### Frontend
- **Framework**: Svelte
- **Zukünftig**: Windows Explorer Integration (ähnlich OneDrive)

### Infrastructure
- **Persistence**: Document-oriented Databases (DynamoDB, Firestore)
- **Search**: Elasticsearch / MongoDB (CQRS Query-Side)
- **Observability**: OpenTelemetry (OTEL)
- **IaC**: OpenTofu (Terraform alternative)
- **OCR**: Cloud-native Services (Google Document AI / AWS Textract)

## Projekt-Prinzipien

- **KISS** (Keep It Simple, Stupid)
- **Clean Code** & **Clean Architecture**
- **Domain-Centric Design**
- **Multi-Cloud Ready**
- **Serverless First**
- **Cost-Efficient** (optimiert für Privatanwender, nicht Netflix-Scale)

## Projektstruktur

```
.
├── docs/                           # Dokumentation
│   ├── arc42/                      # Arc42 Architekturdokumentation
│   └── adr/                        # Architecture Decision Records
└── src/                            # Quellcode
    ├── backend/                    # C# Backend (Core Domain)
    │   ├── PLC.Domain/            # Domain Models, Entities, Value Objects
    │   ├── PLC.Application/       # Use Cases, Application Services
    │   ├── PLC.Infrastructure/    # External Services Integration
    │   ├── PLC.Persistence/       # Data Access, Repositories
    │   ├── PLC.Shared/            # Shared Code, Common Utilities
    │   └── PLC.Api/               # REST API (Controller, DTOs)
    ├── infrastructure/             # Infrastructure as Code
    │   ├── gcp/                    # Google Cloud Platform
    │   │   ├── envs/dev/          # Environment-spezifische Konfiguration
    │   │   └── modules/           # Wiederverwendbare OpenTofu Module
    │   └── aws/                    # Amazon Web Services
    │       ├── envs/dev/
    │       └── modules/
    ├── services/                   # Microservices/Nano-Services
    │   ├── PLC.DocumentScanner/   # OCR und Document Processing
    │   ├── PLC.DocumentClassifier/ # KI-gestützte Klassifizierung
    │   └── PLC.DocumentSearch/    # Volltextsuche Service
    ├── functions/                  # Serverless Functions (Node.js)
    ├── frontend/                   # Svelte Web Application
    └── config/                     # Konfigurationsdateien
        └── document-structure.yaml
```

## Getting Started

### Quick Start

1. **Run Bootstrap Script**
   ```bash
   cd src/infrastructure/gcp/envs/dev
   ./bootstrap.sh
   ```

2. **Follow Setup Guide**

   See [Getting Started Guide](docs/getting-started.md) for detailed instructions.

3. **Start Development**
   ```bash
   # Frontend
   cd src/frontend && npm run dev

   # Backend
   cd src/backend/PLC.Api && dotnet run
   ```

## Dokumentation

Die vollständige Architekturdokumentation befindet sich im [docs/arc42](docs/arc42/) Verzeichnis.

Wichtige Architekturentscheidungen werden als ADRs (Architecture Decision Records) im [docs/adr](docs/adr/) Verzeichnis dokumentiert.

## Entwicklung

### Voraussetzungen
- Node.js LTS (v20+)
- .NET 8 SDK
- OpenTofu
- Google Cloud SDK (gcloud CLI)

### Entwicklungsumgebung
- Linux / WSL recommended
- VS Code with extensions (C# Dev Kit, Svelte, HashiCorp Terraform)
- Editor mit EditorConfig Support

## Lizenz

MIT Siehe [LICENSE](LICENSE)

## Kontakt

> TBD
