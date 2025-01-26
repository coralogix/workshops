<?php

use Psr\Http\Message\ResponseInterface as Response;
use Psr\Http\Message\ServerRequestInterface as Request;
use Slim\Factory\AppFactory;
use OpenTelemetry\SDK\Trace\TracerProviderFactory;

require __DIR__ . '/vendor/autoload.php';

/**
 * Check if the ctype PHP extension is loaded.
 * Log a message indicating whether the extension is available.
 */
error_log(extension_loaded('ctype') ? "INFO: ctype extension is loaded." : "ERROR: ctype extension is NOT loaded.");

/**
 * Create a Slim application instance for handling routes and requests.
 */
$app = AppFactory::create();

/**
 * Route: /rolldice
 * Simulates rolling a dice and returns a random number between 1 and 6.
 * Logs the result of the dice roll for debugging purposes.
 */
$app->get('/rolldice', function (Request $request, Response $response) {
    $result = random_int(1, 6); // Generate a random number between 1 and 6
    error_log("INFO: Rolled a dice, result: $result"); // Log the dice roll result
    $response->getBody()->write(strval($result)); // Write the result to the response body
    return $response; // Return the response
});

/**
 * Route: /test-tracer
 * Tests OpenTelemetry tracing by initializing the TracerProvider locally,
 * starting and ending a span, and logging both Trace ID and Span ID.
 */
$app->get('/test-tracer', function (Request $request, Response $response) {
    error_log("INFO: /test-tracer route invoked."); // Log the invocation of the route

    try {
        /**
         * Initialize the TracerProvider locally using the TracerProviderFactory.
         * This factory automatically configures the provider based on environment variables.
         */
        $tracerProvider = (new TracerProviderFactory())->create();

        /**
         * Retrieve a Tracer instance from the local TracerProvider.
         * The Tracer is used to create and manage spans for tracing operations.
         */
        $tracer = $tracerProvider->getTracer('default');

        /**
         * Create and start a new span using the Tracer.
         * A span represents a unit of work, such as a function call or request.
         */
        $span = $tracer->spanBuilder('test-span')->startSpan();
        $traceId = $span->getContext()->getTraceId(); // Get the Trace ID
        $spanId = $span->getContext()->getSpanId();   // Get the Span ID
        error_log("INFO: Span started successfully: Trace ID: $traceId, Span ID: $spanId");

        /**
         * End the span to mark the completion of the traced operation.
         */
        $span->end();
        error_log("INFO: Span ended successfully.");
    } catch (Throwable $e) {
        /**
         * Log an error message if an exception occurs during tracing.
         * Include the exception message and stack trace for debugging.
         */
        error_log("ERROR: Exception occurred - " . $e->getMessage());
        error_log("ERROR: Stack trace - " . $e->getTraceAsString());
    }

    /**
     * Return a JSON response indicating that the tracer test was successful.
     */
    $response->getBody()->write(json_encode(['status' => 'Tracer tested']));
    return $response->withHeader('Content-Type', 'application/json'); // Set response content type to JSON
});

/**
 * Return the Slim application instance to handle incoming requests.
 */
return $app;
