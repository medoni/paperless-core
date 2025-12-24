# 11 - Benachrichtigungssystem

**Status:** 🚫 Won't Do (POC)
**Meilenstein:** V2
**Priorität:** Niedrig

## Anforderungen

- E-Mail Benachrichtigungen
- Ereignisse: Upload Complete, OCR Failed, Classification Manual Review

## Zu klären

- E-Mail Service: SendGrid, AWS SES, Google Cloud SMTP?
- Template-System: Welches?
- Notification Preferences: User kann aktivieren/deaktivieren?

## Ereignisse (Draft)

- `DocumentProcessed`: "Dein Dokument wurde verarbeitet"
- `OcrFailed`: "OCR fehlgeschlagen, manuelle Prüfung erforderlich"
- `ManualReviewRequired`: "Klassifizierung unsicher, bitte prüfen"
- `MonthlyReport`: "Deine Monatsübersicht ist verfügbar"

## ADR erforderlich

ADR-008: Notification System Design
