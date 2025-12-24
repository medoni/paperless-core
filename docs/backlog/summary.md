# PaperlessCore - Backlog Summary

> **POC-Phase:** Temporäres Backlog bis POC auf GCloud fertig ist.
> **Nach POC:** Migration zu GitHub Issues + Project Board

## Status-Legende

- ⏳ **To Do** - Noch nicht begonnen
- 🔄 **In Progress** - Wird gerade bearbeitet
- ✅ **Done** - Erledigt
- 🚫 **Won't Do (POC)** - Verschoben auf Post-POC

---

## Übersicht

| Nr | Titel | Status | Meilenstein | Priorität |
|----|---|---|---|---|
| 01 | [API-First Approach](entries/v1/01-api-first-approach.md) | ⏳ To Do | V1 | Hoch |
| 02 | [Authentication & Authorization](entries/v1/02-authentication-authorization.md) | 🚫 Won't Do (POC) | V1 | Hoch |
| 03 | [Domain für Testumgebung](entries/v1/03-domain-testumgebung.md) | ⏳ To Do | V1 | Hoch |
| 04 | [Dateinamen & Deep Links](entries/v1/04-dateinamen-deep-links.md) | ⏳ To Do | V1 | Hoch |
| 05 | [Code & Repository Cleanup](entries/v1/05-code-repository-cleanup.md) | ✅ Done | V1 | Hoch |
| 06 | [DateTime Handling Strategy](entries/v1/06-datetime-handling-strategy.md) | ✅ Done | V1 | Hoch |
| 07 | [CI/CD Pipeline](entries/v1/07-ci-cd-pipeline.md) | 🚫 Won't Do (POC) | V1 | Mittel |
| 08 | [Test-Pyramide & Coverage](entries/v1/08-test-pyramide-coverage.md) | 🚫 Won't Do (POC) | V1 | Mittel |
| 09 | [Qualitätsattribute & Szenarien](entries/v1/09-qualitaetsattribute-szenarien.md) | ⏳ To Do | V1 | Mittel |
| 10 | [CLI Frontend](entries/v2/10-cli-frontend.md) | 🚫 Won't Do (POC) | V2 | Niedrig |
| 11 | [Benachrichtigungssystem](entries/v2/11-benachrichtigungssystem.md) | 🚫 Won't Do (POC) | V2 | Niedrig |
| 12 | [Archiv-System für Retention](entries/v2/12-archiv-system-retention.md) | 🚫 Won't Do (POC) | V2 | Niedrig |
| 13 | [Permanente Löschfunktion](entries/v2/13-permanente-loeschfunktion.md) | 🚫 Won't Do (POC) | V2 | Niedrig |
| 14 | [Feinere Zugriffsrechte](entries/v2/14-feinere-zugriffsrechte.md) | ⏳ To Do | V2 | Niedrig |
| 15 | [Backup/Restore-Konzept](entries/v2/15-backup-restore-konzept.md) | ⏳ To Do | V2 | Niedrig |
| 16 | [Cloud Build Multi-Cloud Refactoring](entries/tech-debt/16-cloud-build-multi-cloud-refactoring.md) | ⏳ To Do | Tech Debt | Mittel |

---

## Statistik

### Nach Meilenstein

- **V1**: 9 Einträge (2 erledigt, 3 offen, 4 verschoben)
- **V2**: 6 Einträge (0 erledigt, 2 offen, 4 verschoben)
- **Tech Debt**: 1 Eintrag (0 erledigt, 1 offen)

### Nach Status

- ✅ **Done**: 2 Einträge
- ⏳ **To Do**: 6 Einträge
- 🚫 **Won't Do (POC)**: 8 Einträge

---

## Nächste Architekturentscheidungen (ADRs)

- ADR-004: API Design & Versioning Strategy (→ Eintrag 01)
- ADR-005: Authentication & Authorization Strategy (→ Eintrag 02)
- ADR-006: Testing Strategy & Test Pyramid (→ Eintrag 08)
- ADR-007: CI/CD Pipeline & Deployment Strategy (→ Eintrag 07)
- ADR-008: Notification System Design (→ Eintrag 11)

---

## Update-Log

- 2025-12-24: Backlog in separate Dateien strukturiert (v1/, v2/, tech-debt/)
- 2025-12-23: Status-Tracking für POC-Phase hinzugefügt
- 2025-12-22: Initial Backlog erstellt basierend auf Product Owner Wünsche
