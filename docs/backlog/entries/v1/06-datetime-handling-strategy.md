# 06 - DateTime Handling Strategy

**Status:** ✅ Done
**Meilenstein:** V1
**Priorität:** Hoch

## Anforderungen

- Konsistente Zeitstempel-Behandlung
- Timezone-Awareness
- API-Kompatibilität

## Entscheidung

- **User-facing**: `DateTimeOffset` für alle User-Requests (API, Frontend, Datenbank)
  - Beispiel: Document Upload Timestamp, User Created At
  - Preserves timezone information
- **Server-internal**: `DateTimeOffset.Utc` für Server-Operationen (Jobs, Scheduler, Logs)
  - Beispiel: Background Job Execution, System Events
  - Always UTC for consistency

## Implementierung

```csharp
// User-facing API
public class Document
{
    public DateTimeOffset UploadedAt { get; set; }  // Preserves user timezone
    public DateTimeOffset LastModified { get; set; }
}

// Server-internal
public class BackgroundJob
{
    public DateTimeOffset ExecutedAt => DateTimeOffset.UtcNow;  // Always UTC
}
```

## Begründung

- `DateTimeOffset` über `DateTime` (timezone-aware)
- User-Daten mit Timezone-Info für korrekte Anzeige
- Server-Logs immer UTC für Consistency
- Keine Timezone-Conversion-Bugs
