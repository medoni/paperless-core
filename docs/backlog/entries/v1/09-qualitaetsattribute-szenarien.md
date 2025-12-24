# 09 - Qualitätsattribute & Szenarien

**Status:** ⏳ To Do
**Meilenstein:** V1
**Priorität:** Mittel

## Anforderungen

- Konkrete Qualitätsszenarien definieren
- Messbare Metriken festlegen
- Akzeptanzkriterien

## Performance

- **Szenario**: User uploaded Dokument → UI reagiert sofort
- **Metrik**: Response Time (Upload API)
- **Ziel**: < 2 Sekunden für Upload-Bestätigung (Standard 2025)
- **Async Processing**: Status-Updates via Polling oder WebSocket (V2)

## Verfügbarkeit

- **Szenario**: System ist verfügbar für Privatnutzung
- **Metrik**: Uptime Percentage
- **Ziel**: 99% Uptime (~7h Downtime/Monat akzeptabel)
- **Ausfallzeit**: 1-3 Stunden vertretbar
- **Kein Multi-Region Failover** erforderlich (KISS)
- **Backups**: Ja (regelmäßig, automatisiert)

## Usability

- KISS über perfekte UX (für Alpha)
- Minimalistisches Frontend oder CLI ausreichend
- DAUs (Daily Active Users) erstmal vernachlässigen
- Fokus: Funktionalität über Design

## Sicherheit

- **Szenario**: Unbefugter versucht API-Zugriff
- **Metrik**: Authorization Success Rate
- **Ziel**: 100% korrekte Authentifizierung/Autorisierung
- **OWASP Top 10** beachten

## Kosten

- **Szenario**: Durchschnittlicher Haushalt, 50 Uploads/Monat
- **Metrik**: Monatliche Cloud-Kosten
- **Ziel**: < 10 EUR/Monat
