# Go Auto-Instrumentation Reference Example

This is a reference implementation demonstrating **OpenTelemetry auto-instrumentation** for Go applications in Kubernetes.

## What This Example Demonstrates

### ✅ Auto-Instrumentation Capabilities

- **Automatic distributed tracing** via eBPF instrumentation
- **HTTP request/response tracing** without code changes
- **Kubernetes-native deployment** with operator injection
- **Integration with observability platforms** (Jaeger, Zipkin, etc.)
- **Zero-code instrumentation** for basic tracing

### ❌ Known Limitations

- **Trace context NOT accessible** to application code
- **Direct log/trace correlation** not possible
- `trace.SpanFromContext(ctx)` returns invalid spans
- Limited compared to manual SDK or Java/.NET auto-instrumentation

## Architecture

```
┌─────────────────────────────────────────────────────┐
│ Kubernetes Pod                                      │
├─────────────────────────────────────────────────────┤
│ Auto-Instrumentation Sidecar                       │
│ ┌─────────────────────────────────────────────────┐ │
│ │ eBPF Probes                                     │ │
│ │ - HTTP syscall interception                     │ │
│ │ - Trace generation                              │ │
│ │ - OTLP export                                   │ │
│ └─────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────┤
│ Application Container                               │
│ ┌─────────────────────────────────────────────────┐ │
│ │ Go Application                                  │ │
│ │ - Structured logging                            │ │
│ │ - Request IDs for correlation                   │ │
│ │ - No OpenTelemetry SDK code required            │ │
│ └─────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────┘
```

## How Auto-Instrumentation Works

1. **Kubernetes Operator** injects auto-instrumentation sidecar
2. **eBPF probes** intercept HTTP syscalls from your Go binary
3. **Trace data** is generated and exported to OTLP endpoint
4. **Your application** runs unchanged, with no SDK dependencies

## Deployment

### Prerequisites
- OpenTelemetry Operator installed in cluster
- Instrumentation resource configured

### Deploy
```bash
kubectl apply -f deploy-go-autoinject.yaml
```

### Key Annotations
```yaml
annotations:
  instrumentation.opentelemetry.io/inject-go: "default/instrumentation"
  instrumentation.opentelemetry.io/otel-go-auto-target-exe: "/root/app"
```

## Log Structure

The application produces structured JSON logs with consistent fields:

```json
{
  "timestamp": "2025-07-16T01:58:00.123456789Z",
  "level": "info",
  "message": "HTTP request served",
  "trace_id": "",
  "span_id": "",
  "service_name": "go-autoinject-demo",
  "path": "/api/data",
  "method": "GET",
  "remote_addr": "127.0.0.1:12345",
  "request_id": "req-123456"
}
```

**Note**: `trace_id` and `span_id` will be empty due to auto-instrumentation limitations.

## Correlation Strategies

Since direct trace correlation isn't possible, use these approaches:

### 1. Request ID Correlation
```go
requestID := generateRequestID()
logInfo(ctx, "Request processed", logrus.Fields{
    "request_id": requestID,
    "path": r.URL.Path,
})
```

### 2. Timestamp + Service Correlation
- Correlate logs and traces by timestamp
- Use service name for filtering
- Look for HTTP status codes and paths

### 3. Error Correlation
- HTTP errors appear in both logs and traces
- Use response codes and paths for matching

## Endpoints

The application provides several endpoints for testing:

- `GET /` - Simple hello endpoint
- `GET /api/data` - API endpoint with processing delay
- `GET /api/slow` - Slow endpoint (500ms-2s delay)
- `GET /api/error` - Randomly returns 500 errors (30% chance)
- `GET /health` - Health check endpoint

## Comparison with Manual SDK

| Feature | Auto-Instrumentation | Manual SDK |
|---------|---------------------|------------|
| Setup complexity | Minimal | Moderate |
| Code changes | None | Required |
| Trace context access | ❌ No | ✅ Yes |
| Log correlation | ❌ Limited | ✅ Full |
| Language support | Limited | Full |
| Performance overhead | Low | Variable |

## When to Use Auto-Instrumentation

**Good for:**
- Quick observability wins
- Legacy applications
- Minimal code changes
- Basic distributed tracing

**Consider Manual SDK for:**
- Custom instrumentation needs
- Log/trace correlation requirements
- Complex business logic tracing
- Full OpenTelemetry feature set

## Further Reading

- [OpenTelemetry Go Auto-Instrumentation](https://github.com/open-telemetry/opentelemetry-go-instrumentation)
- [Kubernetes Operator Documentation](https://github.com/open-telemetry/opentelemetry-operator)
- [OpenTelemetry Go Manual SDK](https://opentelemetry.io/docs/languages/go/) 