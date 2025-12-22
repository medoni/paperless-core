# ADR-000: Template für Architecture Decision Records

## Status

Accepted

## Kontext

Architecture Decision Records (ADRs) dokumentieren wichtige Architekturentscheidungen im Projekt. Sie helfen dabei:
- Entscheidungen nachvollziehbar zu machen
- Neuen Teammitgliedern den Kontext zu vermitteln
- Bei späteren Änderungen die ursprünglichen Überlegungen zu verstehen

## Entscheidung

Wir verwenden ADRs nach dem MADR-Format (Markdown Any Decision Records) mit folgender Struktur:

### Sektionen
1. **Status**: Proposed / Accepted / Deprecated / Superseded
2. **Kontext**: Welches Problem wird gelöst? Welche Rahmenbedingungen gibt es?
3. **Entscheidung**: Was wurde entschieden?
4. **Konsequenzen**: Welche Auswirkungen hat die Entscheidung?
5. **Alternativen**: Welche Optionen wurden betrachtet und warum abgelehnt?

### Benennungsschema
- Format: `adr-XXX-kurzer-titel.md`
- Nummerierung: Fortlaufend (001, 002, 003, ...)
- Titel: Kurz, beschreibend, lowercase-mit-bindestrichen

## Konsequenzen

### Positiv
- Transparente Dokumentation von Entscheidungen
- Nachvollziehbarkeit für zukünftige Entwickler
- Grundlage für Diskussionen und Reviews

### Negativ
- Zusätzlicher Dokumentationsaufwand
- Muss gepflegt werden

## Alternativen

- **Keine formale Dokumentation**: Risiko von verlorenem Wissen
- **Wiki/Confluence**: Weniger versioniert, nicht im Repository
- **Code-Kommentare**: Zu fragmentiert für architektonische Entscheidungen
