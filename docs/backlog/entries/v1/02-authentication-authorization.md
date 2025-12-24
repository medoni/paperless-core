# 02 - Authentication & Authorization

**Status:** 🚫 Won't Do (POC)
**Meilenstein:** V1
**Priorität:** Hoch

## Anforderungen

- Google Sign-In (GCP)
- AWS Cognito (AWS)
- Least Privilege Prinzip
- Multi-User Support (Haushaltsmitglieder)

## Entscheidung V1

- Alle Haushaltsmitglieder gleichberechtigt
- Alle sehen alle Dokumente (keine personenbezogene Einschränkung in V1)
- Rollen: `User` (Standard), `Admin` (Löschoperationen, Migration)
- Session-Management: JWT (Bearer Token)

## V2

- Feinere Zugriffsrechte nach `assignedTo`
- Backup/Restore-Konzept

## ADR erforderlich

ADR-005: Authentication & Authorization Strategy
