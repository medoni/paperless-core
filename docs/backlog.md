# PaperlessCore - Backlog & Offene Punkte

Dieses Dokument sammelt offene Anforderungen, Wünsche und zu klärende Punkte.

## Hochpriorität (V1)

### 1. API-First Approach

**Status:** Offen

**Anforderung:**
- API-Spec gemeinsam mit Frontend und Backend definieren (OpenAPI/Swagger)
- Erst Definition, dann parallele Implementierung
- Mocks für parallele Entwicklung

**Nächste Schritte:**
- OpenAPI Spec für Upload, Documents, Search erstellen
- Review mit Product Owner
- Mock Server aufsetzen

**Entscheidung:**
- Tool: Swagger Editor mit VS Code Extension
- KI-unterstützt (z.B. via Claude Code statt manuell YAML fummeln)
- Versioning: `/api/v1/...`
- OpenAPI 3.x Spec im Repo versioniert

---

### 2. Authentication & Authorization

**Status:** Offen, V2 geplant

**Anforderungen:**
- Google Sign-In (GCP)
- AWS Cognito (AWS)
- Least Privilege Prinzip
- Multi-User Support (Haushaltsmitglieder)

**Entscheidung V1:**
- Alle Haushaltsmitglieder gleichberechtigt
- Alle sehen alle Dokumente (keine personenbezogene Einschränkung in V1)
- Rollen: `User` (Standard), `Admin` (Löschoperationen, Migration)
- Session-Management: JWT (Bearer Token)

**V2:**
- Feinere Zugriffsrechte nach `assignedTo`
- Backup/Restore-Konzept

**ADR erforderlich:** Authentication & Authorization Strategy

---

### 3. Domain für Testumgebung

**Status:** Offen, warte auf Eingabe

**Anforderung:**
- Dedizierte Domain für Test/Dev/Staging
- Separate Umgebung von Production

**Entscheidung:**
- Dev: `dev.paperless-core.squirreldev.de`
- Prod: `prod.paperless-core.squirreldev.de`
- Subdomain-Strategie:
  - API: `api.{env}.paperless-core.squirreldev.de`
  - Frontend: `{env}.paperless-core.squirreldev.de`
- SSL: Let's Encrypt (automatisiert via Terraform)
- IaC: Environment als Terraform Module

---

### 4. Dateinamen & Deep Links

**Status:** Offen

**Anforderungen:**
- Frontend: Lesbare IDs/Slugs (z.B. `rewe-kassenbon-2025-12-22`)
- Intern: UUID für Dokumente
- Deep Links: Direkte Verlinkung zu Dokumenten

**Entscheidung:**
- URL-Schema: `/documents/{document-type}/{year}-{merchant}-{description}-{short-uuid}`
- Slug-Format dokumenttyp-abhängig, konfigurierbar via YAML
- Kollision: Short-UUID garantiert Eindeutigkeit

**Beispiele:**
```
Kassenzettel:  /documents/receipt/2025-rewe-groceries-a4f9
Rechnung:      /documents/invoice/2025-telekom-december-b2c3
Lohnzettel:    /documents/salary/2025-12-company-d5e6
Bauunterlagen: /documents/construction/2025-roof-repair-f7g8
```

**Backend:**
- Document ID: UUID (550e8400-e29b-41d4-a716-446655440000)
- Slug wird generiert aus Metadaten + Short-UUID (erste 4 Zeichen)

**YAML Config:**
```yaml
subcategories:
  - id: receipts
    slugPattern: "{year}-{merchant}-{description}-{shortUuid}"
```

---

## Mittlere Priorität (V1 oder V2)

### 5. CI/CD Pipeline

**Status:** Offen

**Anforderungen:**
- Automated Tests
- Automated Deployments
- Multi-Environment (dev, staging, prod)

**Entscheidung:**
- CI/CD Tool: GitHub Actions (Projekt ist auf GitHub)
- Deployment-Strategie: Rolling Deployment (KISS für V1)
- Secrets Management: GitHub Secrets + Cloud Secret Manager

**Pipeline-Stages (Draft):**
1. Lint & Format
2. Unit Tests
3. Integration Tests
4. Build Docker Images
5. Deploy to Staging
6. E2E Tests
7. Manual Approval
8. Deploy to Production

---

### 6. Test-Pyramide & Coverage

**Status:** Offen

**Anforderungen:**
- Test-Strategie definieren
- Coverage-Ziele festlegen
- Test-Typen priorisieren

**Entscheidung:**
- Coverage-Ziel:
  - Geschäftsregeln: 80% (via Behavior Tests oder Integration Tests)
  - Unit Tests: 70% (ausreichend für V1)
  - Bootstrapper/Registration: Nicht erforderlich
- Testing-Frameworks:
  - C#: xUnit + FluentAssertions
  - Node.js: Jest oder Vitest
  - Frontend: Vitest + Playwright (E2E)
- Load Tests (später): k6 + Smoke Tests

**Test-Pyramide:**
```
        /\
       /E2E\         <- Kritische User Flows (Playwright)
      /------\
     /Integr.\      <- API Tests, Business Rules (80%)
    /----------\
   / Unit Tests \   <- Domain Logic (70%)
  /--------------\
```

**Prioritäten:**
1. Integration/Behavior Tests: Geschäftsregeln, Use Cases (80%)
2. Unit Tests: Domain Entities, Value Objects (70%)
3. E2E Tests: Upload, Search, Classification
4. Smoke Tests: Health Checks, Basic Functionality
5. Load Tests (V2): k6 für Performance-Validierung

**KISS beachten:**
- Keine Test-Pyramide für Test-Pyramide
- Pragmatisch: Testen was wichtig ist
- Business Value über Coverage-Metriken

---

### 7. Qualitätsattribute & Szenarien

**Status:** Offen, gehört in Arc42 Section 10

**Anforderungen:**
- Konkrete Qualitätsszenarien definieren
- Messbare Metriken festlegen
- Akzeptanzkriterien

**Entscheidung:**

**Performance:**
- Szenario: User uploaded Dokument → UI reagiert sofort
- Metrik: Response Time (Upload API)
- Ziel: < 2 Sekunden für Upload-Bestätigung (Standard 2025)
- Async Processing: Status-Updates via Polling oder WebSocket (V2)

**Verfügbarkeit:**
- Szenario: System ist verfügbar für Privatnutzung
- Metrik: Uptime Percentage
- Ziel: 99% Uptime (~7h Downtime/Monat akzeptabel)
- Ausfallzeit: 1-3 Stunden vertretbar
- Kein Multi-Region Failover erforderlich (KISS)
- Backups: Ja (regelmäßig, automatisiert)

**Usability:**
- KISS über perfekte UX (für Alpha)
- Minimalistisches Frontend oder CLI ausreichend
- DAUs (Daily Active Users) erstmal vernachlässigen
- Fokus: Funktionalität über Design

**Sicherheit:**
- Szenario: Unbefugter versucht API-Zugriff
- Metrik: Authorization Success Rate
- Ziel: 100% korrekte Authentifizierung/Autorisierung
- OWASP Top 10 beachten

**Kosten:**
- Szenario: Durchschnittlicher Haushalt, 50 Uploads/Monat
- Metrik: Monatliche Cloud-Kosten
- Ziel: < 10 EUR/Monat

---

## Niedrige Priorität (V2+)

### 8. CLI Frontend

**Status:** V2, Nice-to-Have

**Anforderungen:**
- Command-Line Interface als alternative Frontend
- Funktionen: Upload, Search, List, Export

**Beispiel-Commands:**
```bash
plc upload receipt.pdf --category receipts --tags "rewe,2025"
plc search --query "REWE" --year 2025
plc list --category receipts --year 2025
plc export --format csv --year 2024 --category receipts
```

**Technologie:**
- Node.js CLI (Commander.js, Inquirer)
- Oder Go CLI (Cobra)

---

### 9. Benachrichtigungssystem

**Status:** V2

**Anforderungen:**
- E-Mail Benachrichtigungen
- Ereignisse: Upload Complete, OCR Failed, Classification Manual Review

**Zu klären:**
- E-Mail Service: SendGrid, AWS SES, Google Cloud SMTP?
- Template-System: Welches?
- Notification Preferences: User kann aktivieren/deaktivieren?

**Ereignisse (Draft):**
- `DocumentProcessed`: "Dein Dokument wurde verarbeitet"
- `OcrFailed`: "OCR fehlgeschlagen, manuelle Prüfung erforderlich"
- `ManualReviewRequired`: "Klassifizierung unsicher, bitte prüfen"
- `MonthlyReport`: "Deine Monatsübersicht ist verfügbar"

---

## Entscheidungen getroffen

1. ✅ API-First: Swagger Editor + VS Code Extension, KI-unterstützt
2. ✅ Authentication: V1 alle gleichberechtigt, Rollen User/Admin
3. ✅ Domain: `{env}.paperless-core.squirreldev.de`
4. ✅ Dateinamen: Dokumenttyp-abhängige Slugs via YAML Config
5. ✅ CI/CD: GitHub Actions
6. ✅ Testing: 80% Business Rules, 70% Unit Tests, xUnit/Jest/Vitest
7. ✅ Qualität: Performance (UI), Kosten (< 10 EUR), Verfügbarkeit (99%)
8. ⏳ Benachrichtigungen: V2, noch zu definieren

## Offene Punkte

- Backup/Restore-Konzept (V2)
- Feinere Zugriffsrechte (V2)
- Benachrichtigungen (V2)

---

## Nächste Architekturentscheidungen (ADRs)

- ADR-004: API Design & Versioning Strategy
- ADR-005: Authentication & Authorization Strategy
- ADR-006: Testing Strategy & Test Pyramid
- ADR-007: CI/CD Pipeline & Deployment Strategy
- ADR-008: Notification System Design

---

## Update-Log

- 2025-12-22: Initial Backlog erstellt basierend auf Product Owner Wünsche
