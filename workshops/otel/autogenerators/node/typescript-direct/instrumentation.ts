// instrumentation.ts
import * as opentelemetry from '@opentelemetry/sdk-node';
import { getNodeAutoInstrumentations } from '@opentelemetry/auto-instrumentations-node';
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-proto';
import { Resource } from '@opentelemetry/resources';
import { ATTR_SERVICE_NAME } from '@opentelemetry/semantic-conventions';
// import { OTLPMetricExporter } from '@opentelemetry/exporter-metrics-otlp-proto';
// import { PeriodicExportingMetricReader } from '@opentelemetry/sdk-metrics';

const sdk = new opentelemetry.NodeSDK({
  resource: new Resource({
    [ATTR_SERVICE_NAME]: 'node-service',
    'cx.application.name': 'node-application',
    'cx.subsystem.name': 'node-subsystem'
  }),
  traceExporter: new OTLPTraceExporter({
    // optional - default url is http://localhost:4318/v1/traces
    url: 'https://ingress.cx498.coralogix.com:443/v1/traces',
    // url: 'http://localhost:4318/v1/traces',
    // optional - collection of custom headers to be sent with each request, empty by default
    headers: {Authorization: 'Bearer YOURKEYHERE'}
  }),
  // metricReader: new PeriodicExportingMetricReader({
  //   exporter: new OTLPMetricExporter({
  //     url: '<your-otlp-endpoint>/v1/metrics', // url is optional and can be omitted - default is http://localhost:4318/v1/metrics
  //     headers: {}, // an optional object containing custom headers to be sent with each request
  //   }),
  // }),
  instrumentations: [getNodeAutoInstrumentations()],
});
sdk.start();