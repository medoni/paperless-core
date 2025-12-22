# PaperlessCore - Claude Code Anweisungen

## Projekt-Kontext

Du arbeitest am **PaperlessCore (PLC)** Projekt - einer Cloud-native Dokumentenverwaltungslösung für private Haushalte.

## Wichtigste Prinzipien

### 1. NIEMALS Annahmen treffen
- **KRITISCH**: Triff KEINE Entscheidungen ohne Rücksprache
- Bei Unsicherheit: Frage IMMER nach
- Biete Optionen an (A vs B) mit kurzer Gegenüberstellung
- Warte auf Bestätigung vor Implementierung

### 2. KISS (Keep It Simple, Stupid)
- Vermeide Over-Engineering
- Einfache Lösungen bevorzugen
- Nur implementieren, was wirklich benötigt wird
- Keine hypothetischen "Nice-to-have" Features

### 3. Clean Architecture & Clean Code
- Domain-centric Design
- Repository Pattern für Datenzugriff
- Klare Trennung: Controller -> Service -> Repository
- SOLID Prinzipien beachten

### 4. Dokumentation ist Pflicht
- **Arc42**: Kontinuierlich pflegen, niemals vernachlässigen
- **ADRs**: Für JEDE wichtige Architekturentscheidung
- **Code-Kommentare**: Nur wo nötig (self-documenting code bevorzugen)
- TBD-Marker setzen, wenn noch nicht klar

## Technologie-Stack

### Backend
- **Core Domain**: C# .NET 8
  - Namespace: `PLC.Domain`, `PLC.Application`, etc.
  - Klassische Controller-Service-Trennung
  - Repository-Abstraktion (für DB-Austausch)

- **Services**: Node.js LTS
  - Serverless Functions
  - Naming: `PLC.DocumentScanner`, `PLC.DocumentClassifier`, etc.

### Frontend
- **Framework**: Svelte
- **Namespace**: `PLC.Web`

### Cloud & Infrastructure
- **Primär**: Google Cloud Platform
- **Sekundär**: AWS
- **Optional**: K3s (self-hosted)
- **IaC**: Terraform
- **Observability**: OpenTelemetry (OTEL)

### Datenbank
- **Write Model**: Firestore (GCP) / DynamoDB (AWS)
- **Read Model**: Elasticsearch oder MongoDB (CQRS)
- **Wichtig**: Immer Repository-Abstraktion verwenden

## Naming Conventions

### Namespaces & Assemblies
- Format: `PLC.<Component>`
- Beispiele:
  - `PLC.Domain`
  - `PLC.Application`
  - `PLC.Infrastructure`
  - `PLC.Persistence`
  - `PLC.Shared`
  - `PLC.Api`

### Services (nach Funktion)
- `PLC.DocumentScanner`
- `PLC.DocumentClassifier`
- `PLC.DocumentSearch`

### Verzeichnisstruktur
```
/src
  /backend          - C# Core Domain
  /services         - Microservices/Nano-Services
  /functions        - Serverless Functions (Node.js)
  /frontend         - Svelte Web App
/docs
  /arc42           - Architekturdokumentation
  /adr             - Architecture Decision Records
/infrastructure
  /<cloud-provider>
    /envs/default
    /modules
```

## Entwicklungs-Workflow

### 1. Vor jeder Implementierung
- Prüfe: Ist Architekturentscheidung nötig? → ADR erstellen
- Prüfe: Betrifft es die Architektur? → Arc42 aktualisieren
- Prüfe: Mehrere Lösungswege möglich? → Mit Product Owner besprechen

### 2. Während der Implementierung
- Clean Code schreiben
- KISS-Prinzip beachten
- Repository Pattern einhalten
- OTEL-Integration berücksichtigen

### 3. Nach der Implementierung
- Dokumentation aktualisieren
- ADR erstellen falls nötig
- Arc42 Abschnitte aktualisieren
- TODOs für offene Punkte dokumentieren

## Code-Style

### C#
- .NET 8 Konventionen
- 4 Spaces Indentation
- Controller-Service-Repository Pattern
- Async/Await wo sinnvoll
- Dependency Injection

### JavaScript/Node.js
- 2 Spaces Indentation
- ESM (ES Modules)
- Async/Await
- Funktional wo möglich

### Allgemein
- Siehe `.editorconfig` für Details
- Self-documenting code bevorzugen
- Kommentare nur wo nötig (Why, not What)

## Architektur-Entscheidungen

### Immer ADR erstellen für
- Wahl einer Technologie/Framework
- Architektur-Pattern Entscheidungen
- Cloud-Service Auswahl
- Datenbank-Schema Änderungen
- Security-relevante Entscheidungen
- Performance-kritische Design-Entscheidungen

### ADR-Prozess
1. Template kopieren: `docs/adr/adr-000-template.md`
2. Nächste Nummer vergeben: `adr-001-...`
3. Alle Sektionen ausfüllen (Status, Kontext, Entscheidung, Konsequenzen, Alternativen)
4. `docs/adr/README.md` aktualisieren
5. In `docs/arc42/09_architecture_decisions.md` referenzieren

## Arc42 Dokumentation

### TBD-Marker
- Setze `> TBD` für noch nicht ausgefüllte Abschnitte
- Füge Kommentar hinzu, was noch fehlt
- Beispiel: `> TBD - wird nach Architektur-Workshop ausgefüllt`

### Kontinuierliche Pflege
- Halte Dokumentation aktuell
- Keine veralteten Informationen
- Bei Änderungen: Sofort dokumentieren

### Stil-Richtlinien für Dokumentation

**Zielgruppe: Entwickler, nicht Marketing**

- Sachlich und präzise schreiben
- Kein KI-Bullshit oder Marketing-Sprech
- Sinnlose Phrasen weglassen

**Zahlen und Schätzungen:**
- Grobe, sinnvolle Rundungen verwenden
- Beispiel: 3.500 PDFs statt 3.195 PDFs
- Beispiel: ~10 EUR/Monat statt 9,73 EUR/Monat

**Emojis und Formatierung:**
- Emojis nur sparsam und gezielt einsetzen
- ✅ nur bei besonderen Highlights im Fließtext
- NICHT bei jeder Zeile in Listen
- Wenn Überschrift "Positiv" heißt, braucht es kein ✅ vor jedem Punkt
- Bold (**fett**) nur für wirklich wesentliche Begriffe
- Nicht jeden zweiten Begriff fett machen

**Listen:**
- Klare, knappe Bullet Points
- Keine redundanten Marker (Überschrift reicht)
- Pro/Con-Listen ohne Emojis vor jedem Item

**Konsequenzen in ADRs:**
- Positiv/Negativ/Risiken klar trennen
- Sachlich auflisten, keine Verkaufsrhetorik
- Mitigation konkret benennen

## Security & Best Practices

### OWASP Top 10 beachten
- Input Validation
- SQL Injection Prevention (via Repository Pattern)
- XSS Prevention
- CSRF Protection
- Secrets Management (keine Credentials in Code)

### DSGVO
- Personenbezogene Daten verschlüsseln
- Datenminimierung
- Recht auf Löschung implementieren

### Performance
- Serverless-optimiert (Cold Start berücksichtigen)
- Kosteneffizient (Pay-per-Use)
- Caching wo sinnvoll

## Testing

### Strategie (noch zu definieren)
- Unit Tests (TBD)
- Integration Tests (TBD)
- E2E Tests (TBD)

## Produktowner-Kommunikation

### Bei Entscheidungen
- Optionen klar gegenüberstellen
- Vor- und Nachteile benennen
- Empfehlung geben (wenn möglich)
- Auf Bestätigung warten

### Vorschläge machen
- Basierend auf 10+ Jahren Erfahrung mit papierlosem Büro
- Use Cases konkret beschreiben
- Machbarkeit einschätzen

### Marketing-Aspekt berücksichtigen
- Implementierung dokumentieren
- Blog-Material sammeln
- Best Practices festhalten

## Wichtige Kommandos

- `/adr` - Neues ADR erstellen
- `/doc` - Arc42 Abschnitt aktualisieren
- `/arch-review` - Architektur-Review durchführen
- `/service-scaffold` - Neuen Service anlegen
- `/todo-review` - Projekt-Status überprüfen

## Zusammenfassung: Die Goldenen Regeln

1. **KEINE Annahmen ohne Rücksprache**
2. **KISS über alles**
3. **Dokumentation ist Pflicht**
4. **Repository Pattern einhalten**
5. **OTEL von Anfang an**
6. **Clean Architecture, keine Shortcuts**
7. **Bei Unsicherheit: FRAGEN**
