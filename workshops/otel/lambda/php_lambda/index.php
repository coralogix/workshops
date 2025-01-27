<?php

use Psr\Http\Message\ResponseInterface as Response;
use Psr\Http\Message\ServerRequestInterface as Request;
use Slim\Factory\AppFactory;
use OpenTelemetry\SDK\Trace\TracerProviderFactory;
use OpenTelemetry\Contrib\Otlp\OtlpHttpTransportFactory;
use OpenTelemetry\SDK\Trace\SpanProcessor\BatchSpanProcessor;
use OpenTelemetry\SDK\Trace\SpanExporter\SpanExporterFactoryInterface;
use OpenTelemetry\Contrib\Otlp\SpanExporter;
use OpenTelemetry\SDK\Trace\TracerProvider;
use OpenTelemetry\SDK\Trace\SpanProcessor\SimpleSpanProcessor;

require __DIR__ . '/vendor/autoload.php';

/**
 * Step 1: Log script initialization.
 */
error_log("INFO: Script initialized.");

/**
 * Step 2: Check if the required PHP extension 'ctype' is loaded.
 */
if (extension_loaded('ctype')) {
    error_log("INFO: ctype PHP extension is loaded.");
} else {
    error_log("ERROR: ctype PHP extension is NOT loaded. This may cause issues.");
}

/**
 * Step 3: Create a Slim application instance for routing and request handling.
 */
try {
    $app = AppFactory::create();
    error_log("INFO: Slim application instance created successfully.");
} catch (Throwable $e) {
    error_log("ERROR: Failed to create Slim application instance - " . $e->getMessage());
    exit(255); // Exit with error code 255
}

/**
 * Step 4: Define the /test-tracer route.
 * This route tests OpenTelemetry tracing by initializing the TracerProvider,
 * creating a span, and exporting it to the OTLP endpoint. Each step is logged.
 */
$app->get('/test-tracer', function (Request $request, Response $response) {
    error_log("INFO: /test-tracer route invoked."); // Log route invocation

    try {
        // Step 4.1: Retrieve OTLP endpoint from the environment variable
        $otlpEndpoint = getenv('OTEL_EXPORTER_OTLP_TRACES_ENDPOINT');
        if (!$otlpEndpoint) {
            error_log("ERROR: OTEL_EXPORTER_OTLP_TRACES_ENDPOINT environment variable is not set.");
            return $response->withStatus(500)->write(json_encode(['error' => 'OTLP endpoint not configured.']));
        }
        error_log("INFO: OTLP endpoint: $otlpEndpoint");

        // Step 4.2: Initialize OTLP HTTP Transport
        error_log("INFO: Initializing OTLP HTTP Transport...");
        // Fetch the endpoint directly from the environment variable, defaulting to the full path if not set.
        $otlpEndpoint = getenv('OTEL_EXPORTER_OTLP_TRACES_ENDPOINT') ?: 'http://127.0.0.1:4318/v1/traces';
        // Initialize the OTLP transport with the environment-provided or default endpoint.
        $transport = (new OtlpHttpTransportFactory())->create($otlpEndpoint, 'application/x-protobuf');
        error_log("INFO: OTLP HTTP Transport initialized successfully.");
        

        // Step 4.3: Initialize OTLP Span Exporter
        error_log("INFO: Initializing OTLP Span Exporter...");
        $exporter = new SpanExporter($transport);
        error_log("INFO: OTLP Span Exporter initialized successfully.");

        // Step 4.4: Initialize BatchSpanProcessor
        // error_log("INFO: Initializing BatchSpanProcessor...");
        // $spanProcessor = new BatchSpanProcessor($exporter);
        // error_log("INFO: BatchSpanProcessor initialized successfully.");

        // Step 4.5: Initialize TracerProvider
        error_log("INFO: Initializing TracerProvider...");
        $tracerProvider =  new TracerProvider(
            new SimpleSpanProcessor(
                $exporter
            )
        );
        error_log("INFO: TracerProvider initialized successfully.");

        // Step 4.6: Retrieve a Tracer instance
        error_log("INFO: Retrieving Tracer instance...");
        // $tracer = $tracerProvider->getTracer('default');
        $tracer = $tracerProvider->getTracer('io.opentelemetry.contrib.php');
        error_log("INFO: Tracer instance retrieved successfully.");

        // Step 4.7: Start a new span
        error_log("INFO: Starting new span...");
        try {
            error_log("INFO: Preparing to build span...");
            $spanBuilder = $tracer->spanBuilder('test-span');
            error_log("INFO: SpanBuilder instance created.");

            error_log("INFO: Starting the span...");
            $span = $spanBuilder->startSpan();
            error_log("INFO: Span started successfully.");

            // Log span context details
            $traceId = $span->getContext()->getTraceId();
            $spanId = $span->getContext()->getSpanId();
            error_log("INFO: Span Context: Trace ID: $traceId, Span ID: $spanId");

            // Add attributes to the span
            error_log("INFO: Adding attributes to the span...");
            $span->setAttribute('example.attribute', 'test-value');
            error_log("INFO: Attribute 'example.attribute' set to 'test-value'.");

            // Add events to the span
            error_log("INFO: Adding event to the span...");
            $span->addEvent('Span event: test-event');
            error_log("INFO: Event 'test-event' added to the span.");

            // End the span
            error_log("INFO: Ending the span...");
            $span->end();
            error_log("INFO: Span ended successfully.");
        } catch (Throwable $e) {
            error_log("ERROR: Exception occurred while managing span - " . $e->getMessage());
            error_log("ERROR: Stack trace - " . $e->getTraceAsString());
            throw $e; // Re-throw to handle in the outer catch block
        }

        // Step 4.8: Shutdown TracerProvider
        error_log("INFO: Shutting down TracerProvider...");
        $tracerProvider->shutdown();
        error_log("INFO: TracerProvider shut down successfully. Spans exported.");
    } catch (Throwable $e) {
        // Step 4.9: Log errors with detailed information
        error_log("ERROR: Exception occurred during tracing - " . $e->getMessage());
        error_log("ERROR: Stack trace - " . $e->getTraceAsString());
        return $response->withStatus(500)->write(json_encode(['error' => 'Tracing failed.']));
    }

    // Step 4.10: Return success response
    error_log("INFO: /test-tracer completed successfully.");
    $response->getBody()->write(json_encode(['status' => 'Tracer tested and spans exported']));
    return $response->withHeader('Content-Type', 'application/json');
});

/**
 * Step 5: Return the Slim application instance for AWS Lambda to invoke.
 */
error_log("INFO: Returning Slim application instance for AWS Lambda.");
return $app;
