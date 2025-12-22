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
- **IaC**: Terraform
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
├── infrastructure/                 # Infrastructure as Code
│   ├── gcp/                        # Google Cloud Platform
│   │   ├── envs/default/          # Environment-spezifische Konfiguration
│   │   └── modules/               # Wiederverwendbare Terraform Module
│   └── aws/                        # Amazon Web Services
│       ├── envs/default/
│       └── modules/
└── src/                            # Quellcode
    ├── backend/                    # C# Backend (Core Domain)
    │   ├── PLC.Domain/            # Domain Models, Entities, Value Objects
    │   ├── PLC.Application/       # Use Cases, Application Services
    │   ├── PLC.Infrastructure/    # External Services Integration
    │   ├── PLC.Persistence/       # Data Access, Repositories
    │   ├── PLC.Shared/            # Shared Code, Common Utilities
    │   └── PLC.Api/               # REST API (Controller, DTOs)
    ├── services/                   # Microservices/Nano-Services
    │   ├── PLC.DocumentScanner/   # OCR und Document Processing
    │   ├── PLC.DocumentClassifier/ # KI-gestützte Klassifizierung
    │   └── PLC.DocumentSearch/    # Volltextsuche Service
    ├── functions/                  # Serverless Functions (Node.js)
    └── frontend/
        └── PLC.Web/               # Svelte Web Application
```

## Getting Started

> Dokumentation folgt während der Entwicklung

## Dokumentation

Die vollständige Architekturdokumentation befindet sich im [docs/arc42](docs/arc42/) Verzeichnis.

Wichtige Architekturentscheidungen werden als ADRs (Architecture Decision Records) im [docs/adr](docs/adr/) Verzeichnis dokumentiert.

## Entwicklung

### Voraussetzungen
- Node.js LTS
- .NET 8 SDK
- Terraform
- Google Cloud SDK / AWS CLI

### Entwicklungsumgebung
- Linux / WSL
- Editor mit EditorConfig Support

## Lizenz

> TBD

## Kontakt

> TBD
