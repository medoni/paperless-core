# 05 - Code & Repository Cleanup

**Status:** ✅ Done
**Meilenstein:** V1
**Priorität:** Hoch

## Anforderungen

- Infrastruktur-Code unter `src/` organisieren
- Veraltete Verzeichnisstrukturen entfernen
- Konsistente Projektstruktur sicherstellen

## Entscheidung

- `/infrastructure` → `/src/infrastructure` verschoben
- `infrastructure/gcp/envs/default` entfernt (verwende `dev` statt `default`)
- Alle Dokumentations-Referenzen aktualisiert
- Bootstrap-Script angepasst für neue Pfade

## Begründung

- Alle Quellcode-Artefakte unter `src/` vereinheitlicht
- Klarere Trennung zwischen Docs und Source
- Environment-Namen explizit (dev, prod) statt generisch (default)
