// Simple Coralogix Metrics Sender
// Sends random metrics to Coralogix via gRPC

import { MeterProvider, PeriodicExportingMetricReader } from '@opentelemetry/sdk-metrics';
import { OTLPMetricExporter } from '@opentelemetry/exporter-metrics-otlp-grpc';
import { Resource } from '@opentelemetry/resources';
import { metrics } from '@opentelemetry/api';

class SimpleCoralogixSender {
  private counter: any = null;
  private count: number = 0;
  private metricName: string = 'simple_counter';
  private appName: string = 'simple-app';
  private subsystemName: string = 'simple-metrics';

  constructor() {
    this.setupCoralogix();
  }

  private setupCoralogix(): void {
    const privateKey = process.env.CORALOGIX_PRIVATE_KEY || "cxtp_KnFCgNvfEHfwgK4AZTImV63yQ1VE7u";
    
    // Create exporter
    const exporter = new OTLPMetricExporter({
      url: "https://ingress.cx498.coralogix.com:443",
      headers: {
        'Authorization': `Bearer ${privateKey}`,
        'cx-application-name': this.appName,
        'cx-subsystem-name': this.subsystemName,
      },
    });

    // Create meter provider
    const meterProvider = new MeterProvider({
      resource: new Resource({
        'service.name': 'simple-service',
      }),
    });

    // Add metric reader
    meterProvider.addMetricReader(new PeriodicExportingMetricReader({
      exporter: exporter,
      exportIntervalMillis: 3000, // Export every 3 seconds
    }));

    // Set global meter provider
    metrics.setGlobalMeterProvider(meterProvider);
    
    // Create counter
    this.counter = metrics.getMeter(this.appName).createCounter(this.metricName, {
      description: 'A simple counter',
    });

    console.log('✅ Coralogix setup complete');
    console.log(`📊 Metric Name: ${this.metricName}`);
    console.log(`📱 Application: ${this.appName}`);
    console.log(`🔧 Subsystem: ${this.subsystemName}`);
  }

  private generateValue(): number {
    return Math.floor(Math.random() * 10) + 1;
  }

  public sendMetric(): void {
    this.count++;
    const value = this.generateValue();
    const time = new Date().toLocaleTimeString();

    // Send to Coralogix
    this.counter.add(value, {
      metric_id: this.count.toString(),
      timestamp: time,
    });

    // Display locally with metric name
    console.log(`📊 ${this.metricName} #${this.count} | Time: ${time} | Value: ${value} | Sent to Coralogix`);
  }

  public start(): void {
    console.log('🚀 Simple Coralogix Metrics Sender');
    console.log(`📊 Sending ${this.metricName} every 3 seconds`);
    console.log(`🔍 Look for in Coralogix: ${this.metricName} (${this.appName}/${this.subsystemName})`);
    console.log('⏹️  Press Ctrl+C to stop\n');

    // Send first metric immediately
    this.sendMetric();

    // Send metrics every 3 seconds
    setInterval(() => {
      this.sendMetric();
    }, 3000);
  }

  public stop(): void {
    console.log(`\n🛑 Stopped! Total metrics sent: ${this.count}`);
    console.log(`🔍 Metric name: ${this.metricName}`);
    console.log(`📱 Application: ${this.appName}`);
    console.log(`🔧 Subsystem: ${this.subsystemName}`);
  }
}

// Create and start
const sender = new SimpleCoralogixSender();

// Handle shutdown
process.on('SIGINT', () => {
  sender.stop();
  process.exit(0);
});

// Start sending metrics
sender.start(); 