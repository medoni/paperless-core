# ADR-001: Document Categorization Strategy

## Status

Accepted

## Kontext

PaperlessCore muss Haushaltsdokumente strukturiert organisieren. Die Herausforderungen:

1. **Verschiedene Dokumenttypen**: Von Kassenzetteln bis Eigentumsverträgen
2. **Mehrere Personen**: 4 Personen im Haushalt (2 Erwachsene, 2 Kinder)
3. **Gemischte Zuordnungen**: Manche Dokumente sind personenbezogen (Gehalt), andere familienbezogen (Haus)
4. **Langfristige Aufbewahrung**: 10+ Jahre, gesetzliche Aufbewahrungsfristen
5. **Migration**: 3.195 bestehende PDFs müssen später migriert werden
6. **Skalierbarkeit**: Struktur soll für viele Haushalte passen (nicht nur einen)

### Anforderungen

- Standard-Struktur für deutsche Haushalte
- Flexibel genug für verschiedene Haushaltskonstellationen
- Später anpassbar (V2: Templates + Setup-Assistent)
- KISS-Prinzip: Nicht over-engineeren
- Basierend auf 10+ Jahren Praxiserfahrung mit papierlosem Büro

## Entscheidung

### 1. Kategorisierungs-Schema

Wir definieren **9 Top-Level Kategorien** mit Subkategorien:

1. **Arbeit & Einkommen** (Work & Income)
2. **Steuern & Finanzamt** (Taxes)
3. **Wohnen & Immobilien** (Housing & Real Estate)
4. **Versicherungen** (Insurance)
5. **Besitz & Vermögen** (Assets & Property)
6. **Alltag & Konsum** (Everyday & Consumption)
7. **Behörden & Anträge** (Authorities & Applications)
8. **Gesundheit** (Health)
9. **Sonstiges** (Miscellaneous)

Jede Kategorie enthält spezifische Subkategorien (siehe `src/config/document-structure.yaml`).

### 2. Hybrides Personenzuordnungs-Modell

Vier Assignment Modes je nach Dokumenttyp:

- **`none`**: Keine Personenzuordnung (Familie/Shared)
  - Beispiel: Haus, Hausratversicherung

- **`single`**: Genau eine Person erforderlich
  - Beispiel: Gehaltsbescheinigungen, Krankenversicherung

- **`multiple`**: Mehrere Personen möglich
  - Beispiel: Manche Versicherungen

- **`optional`**: Personenzuordnung flexibel
  - Beispiel: Finanzamt (Zusammenveranlagung vs. Einzelveranlagung)

**Technische Umsetzung**:
```yaml
Document:
  assignedTo: ["Micha"]  # Array, kann leer sein bei mode=none
  tags: ["2024", "tax", "important"]  # Zusätzliche flexible Tags
```

### 3. YAML-basierte Konfiguration

Zentrale Konfigurationsdatei: `src/config/document-structure.yaml`

**Enthält:**
- Kategorie-Definitionen
- Assignment Modes pro Kategorie/Subkategorie
- Verarbeitungsregeln (OCR, Extraktion, Volltext)
- Aufbewahrungsfristen
- Required Tags
- Extraction Fields (dokumenttyp-spezifisch)

**Vorteile:**
- Getrennt von Code (Infrastructure und Backend können lesen)
- Versionierbar
- Später erweiterbar für Templates (V2)
- Validierbar

### 4. Dokumenttyp-spezifische Verarbeitung

Jeder Dokumenttyp definiert seine Verarbeitung:

```yaml
subcategories:
  - id: receipts
    ocrEnabled: true
    fullTextSearch: true
    dataExtraction: true
    extractionFields: [date, merchant, totalAmount, vatRates]
```

**Beispiele:**
- **Kassenbons**: OCR + Volltext + Detaillierte Extraktion
- **Gehaltsbescheinigungen**: Nur Klassifizierung (V1)
- **Bauunterlagen**: OCR + Volltext, keine strukturierte Extraktion

### 5. Aufbewahrungsfristen

Gesetzliche/sinnvolle Fristen je Dokumenttyp:

- Kassenbons: 2 Jahre (Garantie)
- Steuer: 10 Jahre (gesetzlich)
- Rente: 50 Jahre (Lebenslang)
- Eigentum: 50 Jahre (permanent)

## Konsequenzen

### Positiv

- Universell einsetzbar: Struktur passt für viele deutsche Haushalte
- Flexibel: Hybrid-Modell deckt alle Szenarien ab (persönlich/familiär/optional)
- KISS: Klar strukturiert, nicht over-engineered
- Erweiterbar: YAML-Config ermöglicht einfache Anpassungen
- Migration-freundlich: Bestehende Ordnerstruktur kann gemappt werden
- Praxiserprobt: Basierend auf 10+ Jahren realer Nutzung
- Code-getrennt: Config in `/src/config`, von Infrastructure + Backend nutzbar

### Negativ

- Kein Multi-Tenancy: V1 ist Single-Household (für MVP akzeptabel)
- Deutsch-fokussiert: Kategorien basieren auf deutschen Anforderungen
- Statisch: V1 hat keine Template-Auswahl (kommt in V2)
- YAML-Parsing: Runtime-Overhead beim Lesen (minimal, akzeptabel)

### Risiken

Schema-Änderungen: Änderungen am YAML-Schema erfordern Migration

Mitigation: Schema-Versionierung im YAML (`version: "1.0.0"`), Migration-Scripts

## Alternativen

### Alternative A: Freie Tagging-Struktur

Keine vordefinierten Kategorien, nur flexible Tags.

**Abgelehnt weil:**
- Keine Standardisierung → schlechte UX
- Jeder Haushalt baut eigene Struktur → keine Best Practices
- Schwer zu migrieren zwischen Haushalten

### Alternative B: Hierarchische Ordnerstruktur (wie Dateisystem)

Dokumente in verschachtelten Ordnern organisieren.

**Abgelehnt weil:**
- Unflexibel bei mehrfacher Zuordnung (z.B. Dokument ist "Steuer" UND "Haus")
- Schwer zu durchsuchen
- Refactoring der Struktur ist aufwändig

### Alternative C: Datenbank-only ohne Config-File

Kategorien direkt in Datenbank als Stammdaten.

**Abgelehnt weil:**
- Schlechtere Versionierbarkeit
- Deployment komplexer (Seed-Data erforderlich)
- Schwerer zu reviewen (kein Git-Diff)

### Alternative D: Person als Hierarchie-Ebene

Struktur: `Person > Kategorie > Subkategorie`

**Abgelehnt weil:**
- Familiendokumente (Haus) passen nicht rein
- Zusammenveranlagte Steuern problematisch
- Weniger flexibel als Hybrid-Modell

## Nächste Schritte

1. YAML-Template erstellt (`src/config/document-structure.yaml`)
2. Domain Model definieren (Entity: Document, Category, Person)
3. ADR-002: Upload & Processing Pipeline
4. ADR-003: OCR Strategy

## Referenzen

- `src/config/document-structure.yaml` - Standard-Template
- Bestehendes Setup: ~3.500 PDFs, ~3,3 GB
- ADR-002: Upload and Processing Pipeline (folgt)
- ADR-003: OCR Strategy (folgt)
