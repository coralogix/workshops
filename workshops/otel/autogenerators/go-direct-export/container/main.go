package main

import (
	"context"
	// "log/slog"
	"math/rand"
	"os"
	"time"

	"go.opentelemetry.io/otel"
	"go.opentelemetry.io/otel/exporters/otlp/otlplog/otlploggrpc"
	"go.opentelemetry.io/otel/exporters/otlp/otlpmetric/otlpmetricgrpc"
	"go.opentelemetry.io/otel/log"
	"go.opentelemetry.io/otel/log/global"
	otelLog "go.opentelemetry.io/otel/sdk/log"
	"go.opentelemetry.io/otel/sdk/metric"
	"go.opentelemetry.io/otel/sdk/resource"
	semconv "go.opentelemetry.io/otel/semconv/v1.26.0"
)

// getOTLPEndpoint retrieves the OTLP endpoint from the environment variable.
// If the environment variable is not set, it defaults to a predefined OTLP collector.
func getOTLPEndpoint() string {
	if endpoint := os.Getenv("OTEL_EXPORTER_OTLP_ENDPOINT"); endpoint != "" {
		return endpoint
	}
	return "http://coralogix-opentelemetry-collector:4317" // Default for Kubernetes
}

// setupLogger initializes OpenTelemetry logging and returns a LoggerProvider.
// The logs are exported via OTLP using gRPC with an insecure connection.
func setupLogger(ctx context.Context) (*otelLog.LoggerProvider, error) {
	logExporter, err := otlploggrpc.New(ctx,
		otlploggrpc.WithEndpoint(getOTLPEndpoint()),
		otlploggrpc.WithInsecure(),
	)
	if err != nil {
		return nil, err
	}

	loggerProvider := otelLog.NewLoggerProvider(
		otelLog.WithResource(resource.NewWithAttributes(
			semconv.SchemaURL,
			semconv.ServiceNameKey.String("randomNum-log-metrics-app"),
			semconv.ServiceVersionKey.String("1.0.0"),
		)),
		otelLog.WithProcessor(otelLog.NewBatchProcessor(logExporter)),
	)

	global.SetLoggerProvider(loggerProvider)
	return loggerProvider, nil
}

// setupMetrics initializes OpenTelemetry metrics and returns a MeterProvider.
// The metrics are exported via OTLP using gRPC with an insecure connection.
func setupMetrics(ctx context.Context) (*metric.MeterProvider, error) {
	exporter, err := otlpmetricgrpc.New(ctx,
		otlpmetricgrpc.WithEndpoint(getOTLPEndpoint()),
		otlpmetricgrpc.WithInsecure(),
	)
	if err != nil {
		return nil, err
	}

	meterProvider := metric.NewMeterProvider(
		metric.WithReader(metric.NewPeriodicReader(exporter, metric.WithInterval(1*time.Second))),
		metric.WithResource(resource.NewWithAttributes(
			semconv.SchemaURL,
			semconv.ServiceNameKey.String("random-metrics-app"),
		)),
	)

	otel.SetMeterProvider(meterProvider)
	return meterProvider, nil
}

func main() {
	ctx := context.Background()

	// Seed the random number generator to ensure different outputs in each run.
	rand.Seed(time.Now().UnixNano())

	// Initialize OpenTelemetry Logging
	loggerProvider, err := setupLogger(ctx)
	if err != nil {
		panic("Failed to set up OpenTelemetry logger: " + err.Error())
	}
	defer loggerProvider.Shutdown(ctx)

	// Initialize OpenTelemetry Metrics
	meterProvider, err := setupMetrics(ctx)
	if err != nil {
		panic("Failed to set up OpenTelemetry metrics: " + err.Error())
	}
	defer meterProvider.Shutdown(ctx)

	// Create a meter instance for metrics tracking
	meter := meterProvider.Meter("random-generator")

	// Create an UpDownCounter to track the latest random number
	randomNumberCounter, err := meter.Int64UpDownCounter("latest_random_number")
	if err != nil {
		panic("Failed to create updown counter: " + err.Error())
	}

	// Retrieve OpenTelemetry Logger instance
	otelLogger := global.Logger("random-generator")

	for {
		// Generate a random number between 0-100
		randomNumber := int64(rand.Intn(101))

		// Add the random number to the UpDownCounter metric
		randomNumberCounter.Add(ctx, randomNumber)

		// Create a structured log record
		record := log.Record{}
		record.SetTimestamp(time.Now())
		record.SetSeverity(log.SeverityInfo)
		record.AddAttributes(
			log.String("message", "Otel log generated new random number"),
			log.Int("random_number", int(randomNumber)),
			log.String("service", "random-log-metrics-app"),
			log.String("timestamp", time.Now().Format(time.RFC3339)),
		)
		record.SetBody(log.StringValue("Generated new random number"))

		// Emit the log record using OpenTelemetry
		otelLogger.Emit(ctx, record)

		// Standard output logging (commented out to disable local logging)
		// slog.Info("stdout Generated new random number",
		// 	slog.Int64("random_number", randomNumber),
		// 	slog.String("service", "random-log-metrics-app"),
		// )

		// Wait for 1 second before generating the next number
		time.Sleep(1 * time.Second)
	}
}
