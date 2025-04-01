const { MeterProvider, PeriodicExportingMetricReader } = require('@opentelemetry/sdk-metrics');
const { OTLPMetricExporter } = require('@opentelemetry/exporter-metrics-otlp-grpc');
const { metrics } = require('@opentelemetry/api');
const { credentials } = require('@grpc/grpc-js');

// Set up OTLP exporter to localhost:4317 (adjust if needed)
const exporter = new OTLPMetricExporter({
  url: 'http://localhost:4317',
  credentials: credentials.createInsecure(),
});

// Create the meter provider WITH the metric reader (modern style)
const meterProvider = new MeterProvider({
  readers: [
    new PeriodicExportingMetricReader({
      exporter,
      exportIntervalMillis: 5000, // export every 5 seconds
    }),
  ],
});

// Set global meter provider
metrics.setGlobalMeterProvider(meterProvider);

// Get meter and histogram
const meter = metrics.getMeter('my-meter');
const histogram = meter.createHistogram('request.latency', {
  description: 'Latency of requests',
  unit: 'ms',
});

// Simulate exponential latency values
function getExponentialLatency(mean) {
  return -mean * Math.log(1.0 - Math.random());
}

// Record 20 samples (one per second)
let count = 0;
const interval = setInterval(() => {
  if (count++ >= 20) {
    clearInterval(interval);
    meterProvider.shutdown().then(() => {
      console.log('✅ Metrics exporter shut down');
      process.exit(0);
    });
    return;
  }

  const latency = getExponentialLatency(50);
  histogram.record(latency, { endpoint: '/api/data' });
  console.log(`📊 Recorded latency: ${latency.toFixed(2)} ms`);
}, 1000);