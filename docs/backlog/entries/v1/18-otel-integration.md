# 18 - OpenTelemetry (OTEL) Integration

**Status:** ⏳ To Do
**Meilenstein:** V1
**Priorität:** Hoch

## Anforderungen

- **Distributed Tracing** über alle Services (API, Worker, Frontend)
- **Metrics** für Performance-Monitoring (Request Duration, Error Rates)
- **Correlation** zwischen Logs, Traces und Metrics
- Cloud-Provider Integration (GCP Cloud Trace, AWS X-Ray)
- Multi-Cloud kompatibel (kein Vendor Lock-in)
- Low-Overhead für Production

## Ziele

- **End-to-End Visibility**: Document Upload → Storage → Processing → Notification
- **Performance Debugging**: Wo sind Bottlenecks? (Firestore Query, GCS Upload, etc.)
- **Error Attribution**: Welcher Service verursacht Fehler?
- **SLO Monitoring**: Latency p50, p95, p99 für APIs

## OpenTelemetry Components

### 1. Tracing (Distributed)
- Verfolgt Requests über Service-Grenzen hinweg
- Visualisiert Call-Hierarchie (Parent/Child Spans)
- Misst Latency pro Operation

### 2. Metrics
- Request Count, Duration, Error Rate
- Custom Metrics (Documents Processed, Queue Depth)
- Histograms für Latency-Distribution

### 3. Logs (Correlation)
- Verknüpfung von Logs mit Traces via Trace ID
- Automatische Injektion von Span Context in Logs

## Architektur

```
┌──────────────┐
│   API (.NET) │─┐
└──────────────┘ │
                 │
┌──────────────┐ │    ┌────────────────┐    ┌─────────────────┐
│Worker (Node) │─┼───→│ OTEL Collector │───→│ Cloud Backend   │
└──────────────┘ │    └────────────────┘    │ - GCP Trace     │
                 │                           │ - GCP Metrics   │
┌──────────────┐ │                           │ - AWS X-Ray     │
│Frontend (JS) │─┘                           └─────────────────┘
└──────────────┘
```

## Implementierung

### .NET API (C#)

```csharp
// Program.cs
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddGrpcClientInstrumentation()
        .AddSource("PLC.Api")
        .AddGoogleCloudTraceExporter() // GCP
        .AddOtlpExporter()) // Generic (AWS, etc.)
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddMeter("PLC.Api")
        .AddPrometheusExporter());

// Custom tracing
public class UploadDocumentHandler
{
    private readonly ActivitySource _activitySource = new("PLC.Api");

    public async Task Handle(UploadDocumentCommand cmd)
    {
        using var activity = _activitySource.StartActivity("UploadDocument");
        activity?.SetTag("document.size", cmd.File.Length);
        activity?.SetTag("document.type", cmd.File.ContentType);

        // Your logic here
    }
}
```

### Node.js Worker

```javascript
const { NodeTracerProvider } = require('@opentelemetry/sdk-trace-node');
const { registerInstrumentations } = require('@opentelemetry/instrumentation');
const { HttpInstrumentation } = require('@opentelemetry/instrumentation-http');
const { GrpcInstrumentation } = require('@opentelemetry/instrumentation-grpc');
const { TraceExporter } = require('@google-cloud/opentelemetry-cloud-trace-exporter');

const provider = new NodeTracerProvider();
provider.addSpanProcessor(new BatchSpanProcessor(new TraceExporter()));
provider.register();

registerInstrumentations({
  instrumentations: [
    new HttpInstrumentation(),
    new GrpcInstrumentation(),
  ],
});

// Custom spans
const tracer = trace.getTracer('document-processor');

async function processDocument(docId) {
  const span = tracer.startSpan('processDocument');
  span.setAttribute('document.id', docId);

  try {
    // Processing logic
    span.setStatus({ code: SpanStatusCode.OK });
  } catch (err) {
    span.recordException(err);
    span.setStatus({ code: SpanStatusCode.ERROR });
  } finally {
    span.end();
  }
}
```

### Frontend (Svelte/Browser)

```javascript
import { WebTracerProvider } from '@opentelemetry/sdk-trace-web';
import { DocumentLoadInstrumentation } from '@opentelemetry/instrumentation-document-load';
import { ZipkinExporter } from '@opentelemetry/exporter-zipkin';

const provider = new WebTracerProvider();
provider.addSpanProcessor(new BatchSpanProcessor(new ZipkinExporter()));

provider.register({
  propagator: new W3CTraceContextPropagator(),
});

// Auto-instrument
registerInstrumentations({
  instrumentations: [
    new DocumentLoadInstrumentation(),
    new UserInteractionInstrumentation(),
    new FetchInstrumentation(),
  ],
});
```

## Trace Context Propagation

### HTTP Headers (W3C Standard)
```
traceparent: 00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01
tracestate: congo=t61rcWkgMzE
```

### API → Worker (via Pub/Sub)
```csharp
// API publishes event
var message = new PubsubMessage
{
    Data = ByteString.CopyFromUtf8(json),
    Attributes = {
        ["traceparent"] = Activity.Current?.Id ?? "",
        ["tracestate"] = Activity.Current?.TraceStateString ?? ""
    }
};

// Worker extracts context
var parentContext = Propagators.DefaultTextMapPropagator.Extract(
    default,
    message.Attributes,
    (attrs, key) => attrs.TryGetValue(key, out var value) ? new[] { value } : Array.Empty<string>()
);

using var activity = activitySource.StartActivity(
    "ProcessMessage",
    ActivityKind.Consumer,
    parentContext.ActivityContext
);
```

## Metrics Examples

```csharp
// Counter
var documentUploadCounter = meter.CreateCounter<long>(
    "documents_uploaded_total",
    description: "Total number of documents uploaded"
);
documentUploadCounter.Add(1, new("status", "success"), new("type", "pdf"));

// Histogram
var uploadDurationHistogram = meter.CreateHistogram<double>(
    "document_upload_duration_seconds",
    description: "Duration of document uploads"
);
uploadDurationHistogram.Record(1.234, new("status", "success"));

// Gauge
var queueDepthGauge = meter.CreateObservableGauge(
    "pubsub_queue_depth",
    () => GetQueueDepth()
);
```

## Cloud Backend Configuration

### Google Cloud Trace + Metrics
```csharp
.AddGoogleCloudTraceExporter()
.AddGoogleCloudMetricsExporter()
```

### AWS X-Ray (Multi-Cloud)
```csharp
.AddXRayTraceExporter(options =>
{
    options.Region = "eu-central-1";
})
```

### Generic OTLP (Vendor-neutral)
```csharp
.AddOtlpExporter(options =>
{
    options.Endpoint = new Uri("https://otlp-collector.example.com");
})
```

## Sampling Strategy

```csharp
// Production: Sample 10% für Performance
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetSampler(new TraceIdRatioBasedSampler(0.1)) // 10%
        .AddAspNetCoreInstrumentation(options =>
        {
            // Always sample errors
            options.Filter = (httpContext) =>
                httpContext.Response.StatusCode >= 400 ||
                Random.Shared.NextDouble() < 0.1;
        })
    );
```

## Example: End-to-End Trace

```
Trace ID: 4bf92f3577b34da6a3ce929d0e0e4736
Duration: 1.245s

├─ POST /api/documents (plc-api) - 1.245s
│  ├─ ValidateDocument (plc-api) - 0.012s
│  ├─ UploadToStorage (plc-api) - 0.456s
│  │  └─ GCS.PutObject (gcs-client) - 0.450s
│  ├─ SaveToFirestore (plc-api) - 0.089s
│  │  └─ Firestore.Create (firestore-client) - 0.085s
│  └─ PublishEvent (plc-api) - 0.023s
│     └─ PubSub.Publish (pubsub-client) - 0.020s
│
└─ ProcessDocument (document-processor) - 0.567s [async]
   ├─ FetchDocument (document-processor) - 0.123s
   ├─ ExtractText (document-processor) - 0.389s
   └─ UpdateStatus (document-processor) - 0.055s
```

## Dashboards & Alerts

### Grafana/Cloud Monitoring
- **Golden Signals**: Latency, Traffic, Errors, Saturation
- **SLO Tracking**: 95% of requests < 500ms
- **Error Rate Alerts**: Error rate > 1% triggers PagerDuty

### Example Queries
```promql
# p95 latency
histogram_quantile(0.95,
  sum(rate(document_upload_duration_seconds_bucket[5m])) by (le)
)

# Error rate
sum(rate(documents_uploaded_total{status="error"}[5m]))
/
sum(rate(documents_uploaded_total[5m]))
```

## Begründung

- OTEL ist der Industrie-Standard für Observability (CNCF)
- Vendor-neutral: Einmal instrumentieren, überall exportieren
- Distributed Tracing ist essentiell für Microservices/Event-Driven Architecture
- Hohe Priorität für V1, da Production Debugging ohne Tracing sehr schwierig

## Abhängigkeiten

- ✅ Entry 06: DateTime Handling (für konsistente Timestamps)
- ⏳ Entry 17: Structured Logging (Correlation zwischen Logs & Traces)
- ⏳ Entry 01: API-First Approach (Tracing für API Requests)

## ADR

→ **ADR-010**: Observability & Tracing Strategy
