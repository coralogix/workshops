# OpenTelemetry Java Microservices Demo

A Spring Boot microservices demo application designed to showcase OpenTelemetry trace correlation in realistic performance scenarios.

## Architecture

```
Client Service (Port 8081)               Server Service (Port 8080)
┌─────────────────────────┐              ┌─────────────────────────┐
│ • REST endpoint         │   HTTP       │ • REST endpoint         │
│ • Background loop       │ ──────────►  │ • Realistic slowdowns   │
│ • WebClient calls       │              │ • Multiple frameworks   │
│ • Scheduled requests    │              │ • Performance simulation│
└─────────────────────────┘              └─────────────────────────┘
```

## Applications

### Client Service
- **Endpoint**: `GET /client/trigger` 
- **Background**: Continuous requests to server every 500ms
- **Scheduled**: Automatic traced requests every 5 seconds
- **Purpose**: Generate consistent load and trace correlation

### Server Service  
- **Endpoint**: `GET /api/data`
- **Purpose**: Simulate realistic performance bottlenecks
- **Behavior**: 70% fast responses, 30% slow operations

## Performance Simulation Strategy

### Slowdown Trigger Mechanism
```java
// 30% chance of slow operation
if (random.nextDouble() < 0.3) {
    executeRandomSlowOperation(uuid);
}
```

### Slowdown Method Selection
When triggered (30% of requests), one method is randomly selected:

```java
String[] operations = {
    "http_client",           // 20% of slow requests (6% overall)
    "json_processing",       // 20% of slow requests (6% overall)  
    "cache_loading",         // 20% of slow requests (6% overall)
    "cpu_intensive",         // 20% of slow requests (6% overall)
    "concurrent_processing"  // 20% of slow requests (6% overall)
};
```

## Realistic Slowdown Methods

### 1. HTTP Client Operations (OkHttp)
- **Framework**: OkHttp 4.11.0
- **Operation**: External HTTP calls to `httpbin.org/delay/{1-3}`
- **Duration**: 1-3 seconds
- **Traces**: HTTP client spans, network calls
- **Fallback**: CPU-intensive operations if external calls fail

### 2. Heavy JSON Processing (Jackson)
- **Framework**: Jackson 2.15.2
- **Operation**: Serialize/deserialize 2000+ nested objects (10 iterations)
- **Duration**: 500ms-2s depending on data size
- **Traces**: JSON processing operations
- **Details**: Complex nested structures with deep hierarchies

### 3. Cache Loading Operations (Caffeine)
- **Framework**: Caffeine 2.9.3 (Java 8 compatible)
- **Operation**: Cache misses with expensive computation
- **Duration**: 1-3s for cache population
- **Traces**: Cache operations and computations
- **Details**: 50,000 string operations + SHA-256 hashing

### 4. CPU-Intensive Operations
- **Frameworks**: Java Cryptography, BigInteger
- **Operations**: 
  - SHA-256 hashing (5,000 iterations)
  - BigInteger mathematical operations
  - Prime number calculations up to 1000
  - String manipulations
- **Duration**: 500ms-2s
- **Traces**: CPU-bound processing spans

### 5. Concurrent Processing
- **Framework**: CompletableFuture (Java 8)
- **Operation**: 10 parallel tasks with computation
- **Duration**: 1-2s for all tasks completion
- **Traces**: Concurrent execution spans
- **Details**: Each task performs string building + MD5 hashing

## Java Frameworks & Libraries

### Core Application Frameworks
- **Spring Boot**: 2.7.12 (Web MVC, REST controllers)
- **Spring WebFlux**: WebClient for reactive HTTP calls (client only)
- **Apache Log4j2**: 2.24.3 (Structured logging)

### Performance Simulation Frameworks  
- **OkHttp**: 4.11.0 (HTTP client operations)
- **Jackson**: 2.15.2 (JSON serialization/deserialization)
- **Caffeine**: 2.9.3 (In-memory caching)
- **CompletableFuture**: Java 8 (Concurrent processing)
- **Java Cryptography**: SHA-256, MD5 hashing
- **BigInteger**: Mathematical operations

### OpenTelemetry Integration
- **OpenTelemetry Log4j Context**: 2.11.0-alpha (Trace correlation)
- **Log4j JSON Layout**: 2.24.3 (Structured log output)

## Request Flow & Trace Correlation

### Normal Request (70%)
```
Client → Server → Fast Response (10-50ms)
Logs: trace_id=empty, performance_issue=false
```

### Slow Request (30%)
```
Client → Server → [Random Slow Operation] → Response (500ms-5s)
Logs: trace_id=populated, performance_issue=true, operation_type=specific_method
```

### Log Output Example
```
2025-01-28 12:30:15 [http-nio-8080-exec-1] INFO demo.main.MainApplication$ApiController - Request processed successfully trace_id=12345678901234567890123456789012 span_id=1234567890123456 operation_type=json_processing operation_duration_ms=1247 performance_issue=true
```