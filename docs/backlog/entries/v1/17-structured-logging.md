# 17 - Structured Logging

**Status:** ⏳ To Do
**Meilenstein:** V1
**Priorität:** Hoch

## Anforderungen

- Strukturierte Log-Ausgaben für bessere Durchsuchbarkeit
- Konsistentes Logging über alle Services hinweg
- Integration mit Cloud Logging (GCP) / CloudWatch (AWS)
- Log-Levels und Kategorisierung
- Correlation IDs für Request-Tracking
- Performance-relevante Metrics im Log
- Sensitive Data Masking (PII, Credentials)

## Ziele

- **Debugging**: Schnelles Auffinden von Fehlern in Produktion
- **Monitoring**: Automatische Alerts basierend auf Log-Patterns
- **Compliance**: Nachvollziehbare Audit-Trails
- **Performance**: Low-Overhead Logging mit async Writers

## Technologie-Optionen

### .NET Backend (C#)
- **Microsoft.Extensions.Logging** (Standard)
  - Built-in .NET Framework
  - Strukturiertes Logging mit LoggerMessage Source Generators
  - Providers für Console, Debug, EventSource
  - Erweiterbar mit Google.Cloud.Diagnostics.AspNetCore für Cloud Logging
  - AWS.Logger.AspNetCore für CloudWatch

### Node.js Services
- **Winston** (Empfohlen)
  - JSON-formatted logs
  - Multiple transports (Console, File, Cloud)
- **Pino**
  - Sehr performant, low-overhead

## Implementierung

### Log-Format Standard
```json
{
  "timestamp": "2025-12-24T10:30:00.123Z",
  "level": "Information",
  "message": "Document uploaded successfully",
  "correlationId": "abc-123-def",
  "service": "plc-api",
  "userId": "user-456",
  "documentId": "doc-789",
  "duration_ms": 234,
  "environment": "dev"
}
```

### Microsoft.Extensions.Logging Configuration (C#)
```csharp
// Program.cs
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";
    options.JsonWriterOptions = new JsonWriterOptions { Indented = false };
});

// Google Cloud Logging (GCP)
builder.Logging.AddGoogle(new LoggingServiceOptions
{
    ProjectId = builder.Configuration["GoogleCloud:ProjectId"],
    ServiceName = "plc-api",
    Version = "1.0.0"
});

// AWS CloudWatch (Multi-Cloud)
// builder.Logging.AddAWSProvider(builder.Configuration.GetAWSLoggingConfigSection());

// Add structured logging scopes
builder.Services.AddHttpContextAccessor();
```

### Usage with LoggerMessage Source Generators (High Performance)
```csharp
public partial class UploadDocumentHandler
{
    private readonly ILogger<UploadDocumentHandler> _logger;

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Document {DocumentId} uploaded by user {UserId}, size {FileSize} bytes")]
    private partial void LogDocumentUploaded(string documentId, string userId, long fileSize);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Error,
        Message = "Failed to upload document {DocumentId}")]
    private partial void LogDocumentUploadFailed(string documentId, Exception ex);

    public async Task Handle(UploadDocumentCommand cmd)
    {
        try
        {
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = Activity.Current?.Id ?? Guid.NewGuid().ToString(),
                ["UserId"] = cmd.UserId
            }))
            {
                // Upload logic
                LogDocumentUploaded(document.Id, cmd.UserId, cmd.File.Length);
            }
        }
        catch (Exception ex)
        {
            LogDocumentUploadFailed(cmd.DocumentId, ex);
            throw;
        }
    }
}
```

### Winston Configuration (Node.js)
```javascript
const logger = winston.createLogger({
  level: 'info',
  format: winston.format.combine(
    winston.format.timestamp(),
    winston.format.json()
  ),
  defaultMeta: {
    service: 'document-processor',
    environment: process.env.NODE_ENV
  },
  transports: [
    new winston.transports.Console(),
    new GoogleCloudLoggingTransport()
  ]
});
```

## Log-Levels

- **Critical**: System unusable (Payment failed, DB down)
- **Error**: Feature broken (Document upload failed)
- **Warning**: Degraded state (Retry attempts, fallback used)
- **Information**: Normal operations (Document uploaded, User logged in)
- **Debug**: Detailed diagnostic info (nur in Dev)
- **Trace**: Very detailed (meist deaktiviert)

## Sensitive Data Masking

```csharp
// DO NOT log:
_logger.LogInformation("User password: {Password}", user.Password); // ❌

// DO log:
_logger.LogInformation("User {UserId} authenticated successfully", user.Id); // ✅

// Custom logging middleware for data masking:
public class SensitiveDataLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SensitiveDataLoggingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        // Mask sensitive headers (Authorization, API-Key, etc.)
        var sanitizedHeaders = context.Request.Headers
            .Where(h => !IsSensitiveHeader(h.Key))
            .ToDictionary(h => h.Key, h => h.Value.ToString());

        using (_logger.BeginScope(sanitizedHeaders))
        {
            await _next(context);
        }
    }

    private bool IsSensitiveHeader(string key) =>
        key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) ||
        key.Equals("X-API-Key", StringComparison.OrdinalIgnoreCase);
}
```

## Correlation IDs

- Jeder HTTP Request erhält eine `X-Correlation-ID`
- ID wird durch alle Service-Calls weitergereicht
- Ermöglicht Tracing über Service-Grenzen hinweg (Vorbereitung für OTEL)

## Cloud-spezifische Integration

### Google Cloud Logging (appsettings.json)
```json
{
  "GoogleCloud": {
    "ProjectId": "paperlesscore-dev",
    "LoggingServiceOptions": {
      "ServiceName": "plc-api",
      "Version": "1.0.0",
      "MonitoredResource": {
        "Type": "cloud_run_revision"
      }
    }
  }
}
```

```csharp
// Program.cs
builder.Services.AddGoogleDiagnosticsForAspNetCore();
builder.Logging.AddGoogle(new LoggingServiceOptions
{
    ProjectId = builder.Configuration["GoogleCloud:ProjectId"],
    ServiceName = "plc-api"
});
```

### AWS CloudWatch (Multi-Cloud)
```json
{
  "AWS.Logging": {
    "Region": "eu-central-1",
    "LogGroup": "/aws/cloudrun/plc-api",
    "IncludeScopes": true,
    "IncludeLogLevel": true,
    "IncludeCategory": true
  }
}
```

```csharp
// Program.cs
builder.Logging.AddAWSProvider(builder.Configuration.GetAWSLoggingConfigSection());
```

## Begründung

- Strukturierte Logs sind essentiell für Cloud-native Apps
- JSON-Format ermöglicht effiziente Queries in Cloud Logging/CloudWatch
- Correlation IDs sind Voraussetzung für Distributed Tracing (→ Entry 18)
- Hohe Priorität für V1, da Debugging ohne Logging sehr schwierig

## Abhängigkeiten

- ⏳ Entry 18: OTEL Integration (nutzt gleiche Correlation IDs)
- ⏳ Entry 01: API-First Approach (Logging für API Requests)

## ADR

→ **ADR-009**: Structured Logging Strategy
