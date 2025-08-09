# OpenTelemetry TypeScript Metrics Example

A TypeScript application that demonstrates OpenTelemetry metrics instrumentation. Sends exactly **100 metrics examples** instantly to a local OTLP collector via gRPC on port 4317.

## What It Does

- **Counter Metrics**: Tracks requests with HTTP method, endpoint, and status code labels
- **Histogram Metrics**: Measures response time distributions 
- **Observable Gauge Metrics**: Monitors memory usage (heap, RSS)
- **Instant Execution**: Sends all 100 metrics immediately and stops

## Prerequisites

- Node.js (v16 or higher)
- npm
- Local OpenTelemetry Collector running on port 4317

## Quick Start

1. Install dependencies:
   ```bash
   npm install
   ```

2. Build the application (compiles TypeScript to JavaScript):
   ```bash
   npm run build
   ```

3. Run the application (executes the compiled code and shows console output):
   ```bash
   npm start
   ```

**Note**: `npm run build` only compiles the code and won't show any console output. You must use `npm start` to actually run the application and see the metrics being sent.

## Example Output

When you run `npm start`, you'll see:

```
OpenTelemetry setup complete
Exporting to: http://localhost:4317
Metrics created: counter, histogram, gauge
OpenTelemetry Metrics Example
Sending 100 metrics examples to local OTLP collector
Metrics: demo-requests_total (counter), demo-response_time_ms (histogram), demo-memory_usage_bytes (gauge)

Request #1 | Time: 2:30:15 PM
   GET /api/users - 200 (87ms)
   Counter: +1, Histogram: 87ms, Gauge: auto-updated

Request #2 | Time: 2:30:15 PM
   POST /api/orders - 201 (156ms)
   Counter: +1, Histogram: 156ms, Gauge: auto-updated

... (98 more requests) ...

Request #100 | Time: 2:30:15 PM
   GET /api/users - 200 (126ms)
   Counter: +1, Histogram: 126ms, Gauge: auto-updated

Completed 100 iterations! Stopping...

Metrics Summary:
   Total requests processed: 100
   Counter metrics sent: 100
   Histogram metrics sent: 100
   Gauge metrics: continuously updated
All metrics flushed successfully
```

## Metrics Generated

1. **`demo-requests_total`** (Counter): Total requests with method, endpoint, status_code labels
2. **`demo-response_time_ms`** (Histogram): Response times with custom buckets [10, 50, 100, 200, 500, 1000, 2000, 5000]
3. **`demo-memory_usage_bytes`** (Observable Gauge): Current memory usage by type (heap_used, heap_total, rss)

## Scripts

- `npm run build` - Compile TypeScript to JavaScript (no output expected)
- `npm run dev` - Run with ts-node for development  
- `npm start` - Run compiled JavaScript and see console output

## Configuration

The app sends metrics to `http://localhost:4317`. To change this, edit the `url` in `src/app.ts` and rebuild.