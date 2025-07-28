# OpenTelemetry Demo Server

Simple Spring Boot server that simulates realistic performance issues for OpenTelemetry tracing demonstration.

## Features

- **Single endpoint**: `GET /api/data`
- **Performance simulation**: 30% of requests randomly execute slow operations
- **OpenTelemetry-compatible frameworks**: OkHttp, Jackson, Caffeine, concurrent processing
- **Structured logging**: Log4j2 with trace correlation support

## Quick Start

1. **Build**: `./3-build-app.sh`
2. **Run**: `java -jar target/java-app-1.0-SNAPSHOT.jar`
3. **Test**: `curl http://localhost:8080/api/data`

## With OpenTelemetry Agent

```bash
# Download agent
wget https://github.com/open-telemetry/opentelemetry-java-instrumentation/releases/latest/download/opentelemetry-javaagent.jar

# Run with tracing
java -javaagent:opentelemetry-javaagent.jar \
     -Dotel.service.name=demo-server \
     -Dotel.traces.exporter=logging \
     -jar target/java-app-1.0-SNAPSHOT.jar
```

## Slow Operations (30% chance)

- **HTTP calls**: External requests to httpbin.org/delay
- **JSON processing**: Large object serialization/deserialization  
- **Cache loading**: Expensive computation with Caffeine cache
- **CPU intensive**: Cryptographic hashing and prime calculations
- **Concurrent processing**: Multi-threaded CompletableFuture tasks

## Expected Behavior

- 70% fast responses (~10ms)
- 30% slow responses (500ms-5s)
- Logs include `trace_id` and `span_id` when using OpenTelemetry agent 