package com.example;

import io.opentelemetry.api.OpenTelemetry;
import io.opentelemetry.api.common.Attributes;
import io.opentelemetry.api.metrics.DoubleHistogram;
import io.opentelemetry.api.metrics.Meter;
import io.opentelemetry.exporter.otlp.metrics.OtlpGrpcMetricExporter;
import io.opentelemetry.sdk.OpenTelemetrySdk;
import io.opentelemetry.sdk.metrics.SdkMeterProvider;
import io.opentelemetry.sdk.metrics.export.PeriodicMetricReader;
import io.opentelemetry.sdk.resources.Resource;

import java.time.Duration;
import java.util.Random;
import java.util.concurrent.TimeUnit;


public class ExponentialHistogramExample {

    public static void main(String[] args) throws InterruptedException {
        // Define resource with service name
        Resource resource = Resource.getDefault()
//            .merge(Resource.create(Attributes.of(ResourceAttributes.SERVICE_NAME, "exp-histogram-example")));
            .merge(Resource.create(Attributes.of(io.opentelemetry.api.common.AttributeKey.stringKey("service.name"), "exp-histogram-example")));


        // Set up OTLP exporter to localhost (adjust if needed)
        OtlpGrpcMetricExporter exporter = OtlpGrpcMetricExporter.builder()
            .setEndpoint("http://localhost:4317")  // Change to Coralogix if needed
            .build();

        // Export every 5 seconds like the Python version
        PeriodicMetricReader metricReader = PeriodicMetricReader.builder(exporter)
            .setInterval(Duration.ofSeconds(5))
            .build();

        // Set up the MeterProvider with reader and resource
        SdkMeterProvider meterProvider = SdkMeterProvider.builder()
            .setResource(resource)
            .registerMetricReader(metricReader)
            .build();

        // Get the OpenTelemetry instance
        OpenTelemetry openTelemetry = OpenTelemetrySdk.builder()
            .setMeterProvider(meterProvider)
            .build();

        // Create a Meter
        Meter meter = openTelemetry.getMeter("exp.histogram.meter");

        // Create a Histogram instrument
        DoubleHistogram histogram = meter.histogramBuilder("request.latency")
            .setDescription("Exponential histogram of request latencies")
            .setUnit("ms")
            .build();

        // Simulate and record latency values every second for 20 seconds
        Random random = new Random();
        for (int i = 0; i < 20; i++) {
            double latency = getExponentialSample(random, 50.0);  // mean ~50ms
            histogram.record(latency, Attributes.of(
                io.opentelemetry.api.common.AttributeKey.stringKey("endpoint"), "/api/data"));
            System.out.printf("request.latency: %.2f ms%n", latency);
            Thread.sleep(1000);  // Sleep 1 second
        }

        // Shut down meter provider after all metrics are recorded
        //meterProvider.shutdown().join();
        meterProvider.shutdown().join(5, TimeUnit.SECONDS);
        System.out.println("Done.");
    }

    // Helper: exponential distribution sampling like Python's random.expovariate(1/mean)
    private static double getExponentialSample(Random random, double mean) {
        return -mean * Math.log(1.0 - random.nextDouble());
    }
}