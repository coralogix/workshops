// OpenTelemetry Metrics Example for Coralogix
// Sends various metrics to Coralogix via gRPC

import { MeterProvider, PeriodicExportingMetricReader } from '@opentelemetry/sdk-metrics';
import { OTLPMetricExporter } from '@opentelemetry/exporter-metrics-otlp-grpc';
import { Resource } from '@opentelemetry/resources';
import { metrics, Counter, Histogram, ObservableGauge } from '@opentelemetry/api';
import { ATTR_SERVICE_NAME, ATTR_SERVICE_VERSION } from '@opentelemetry/semantic-conventions';

class MetricsExample {
  private counter!: Counter;
  private histogram!: Histogram;
  private gauge!: ObservableGauge;
  private count: number = 0;
  private meterProvider!: MeterProvider;
  private meter: any;
  private appName: string = 'typescript-metrics-example';
  private subsystemName: string = 'metrics-demo';

  constructor() {
    this.setupOpenTelemetry();
    this.createMetrics();
  }

  private setupOpenTelemetry(): void {
    const privateKey = process.env.CORALOGIX_PRIVATE_KEY || "YOURKEYHERE";
    const appName = process.env.CORALOGIX_APP_NAME || this.appName;
    const subsystemName = process.env.CORALOGIX_SUBSYSTEM || this.subsystemName;

    // Create OTLP gRPC exporter pointing to Coralogix - update the url to the correct for your region
    const exporter = new OTLPMetricExporter({
      url: 'https://ingress.cx498.coralogix.com:443',
      headers: {
        'Authorization': `Bearer ${privateKey}`,
        'cx-application-name': appName,
        'cx-subsystem-name': subsystemName,
      },
    });

    // Create meter provider with resource information
    this.meterProvider = new MeterProvider({
      resource: new Resource({
        [ATTR_SERVICE_NAME]: 'typescript-metrics-example',
        [ATTR_SERVICE_VERSION]: '1.0.0',
        'environment': 'development',
        'team': 'platform'
      }),
    });

    // Add metric reader with 5-second export interval
    this.meterProvider.addMetricReader(new PeriodicExportingMetricReader({
      exporter: exporter,
      exportIntervalMillis: 5000, // Export every 5 seconds
    }));

    // Set global meter provider
    metrics.setGlobalMeterProvider(this.meterProvider);
    
    // Get meter instance
    this.meter = metrics.getMeter('typescript-metrics-example', '1.0.0');

    console.log('OpenTelemetry setup complete');
    console.log(`Exporting to: Coralogix (${appName}/${subsystemName})`);
  }

  private createMetrics(): void {
    // Counter - for counting events
    this.counter = this.meter.createCounter('demo-requests_total', {
      description: 'Total number of requests processed',
      unit: '1'
    });

    // Histogram - for measuring distributions (e.g., response times)
    this.histogram = this.meter.createHistogram('demo-response_time_ms', {
      description: 'Response time in milliseconds',
      unit: 'ms',
      boundaries: [10, 50, 100, 200, 500, 1000, 2000, 5000]
    });

    // Observable Gauge - for current values (e.g., memory usage)
    this.gauge = this.meter.createObservableGauge('demo-memory_usage_bytes', {
      description: 'Current memory usage in bytes',
      unit: 'bytes'
    });

    // Register callback for gauge
    this.gauge.addCallback((result) => {
      const memoryUsage = process.memoryUsage();
      result.observe(memoryUsage.heapUsed, {
        'memory_type': 'heap_used'
      });
      result.observe(memoryUsage.heapTotal, {
        'memory_type': 'heap_total'
      });
      result.observe(memoryUsage.rss, {
        'memory_type': 'rss'
      });
    });

    console.log('Metrics created: counter, histogram, gauge');
  }

  private generateResponseTime(): number {
    // Simulate response times with some variance
    const base = 50;
    const variance = Math.random() * 200;
    return Math.round(base + variance);
  }

  private generateStatusCode(): string {
    const statusCodes = ['200', '201', '400', '404', '500'];
    const weights = [0.7, 0.1, 0.1, 0.05, 0.05]; // 70% success, 30% errors
    
    const random = Math.random();
    let cumulativeWeight = 0;
    
    for (let i = 0; i < statusCodes.length; i++) {
      cumulativeWeight += weights[i];
      if (random <= cumulativeWeight) {
        return statusCodes[i];
      }
    }
    return '200';
  }

  public sendMetrics(): void {
    this.count++;
    const responseTime = this.generateResponseTime();
    const statusCode = this.generateStatusCode();
    const endpoint = Math.random() > 0.5 ? '/api/users' : '/api/orders';
    const method = Math.random() > 0.7 ? 'POST' : 'GET';
    const time = new Date().toLocaleTimeString();

    // Record counter metric
    this.counter.add(1, {
      'method': method,
      'endpoint': endpoint,
      'status_code': statusCode
    });

    // Record histogram metric
    this.histogram.record(responseTime, {
      'method': method,
      'endpoint': endpoint,
      'status_code': statusCode
    });

    // Display metrics being sent
    console.log(`Request #${this.count} | Time: ${time}`);
    console.log(`   ${method} ${endpoint} - ${statusCode} (${responseTime}ms)`);
    console.log(`   Counter: +1, Histogram: ${responseTime}ms, Gauge: auto-updated`);
  }

  public start(): void {
    console.log('OpenTelemetry Metrics Example');
    console.log('Sending 100 metrics examples to Coralogix');
    console.log('Metrics: demo-requests_total (counter), demo-response_time_ms (histogram), demo-memory_usage_bytes (gauge)\n');

    // Send 100 metrics instantly
    for (let i = 0; i < 100; i++) {
      this.sendMetrics();
    }

    console.log(`\nCompleted 100 iterations! Stopping...`);
    this.shutdown();

    // Graceful shutdown for Ctrl+C
    process.on('SIGINT', () => {
      console.log(`\nShutting down gracefully...`);
      this.shutdown();
    });
  }

  private shutdown(): void {
    // Force flush metrics before exit
    this.meterProvider.forceFlush().then(() => {
      console.log(`\nMetrics Summary:`);
      console.log(`   Total requests processed: ${this.count}`);
      console.log(`   Counter metrics sent: ${this.count}`);
      console.log(`   Histogram metrics sent: ${this.count}`);
      console.log(`   Gauge metrics: continuously updated`);
      console.log(`All metrics flushed successfully`);
      process.exit(0);
    }).catch((error) => {
      console.error('Error flushing metrics:', error);
      process.exit(1);
    });
  }
}

// Create and start the metrics example
const metricsExample = new MetricsExample();
metricsExample.start();
