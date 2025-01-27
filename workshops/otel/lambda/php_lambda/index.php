<?php

use Psr\Http\Message\ResponseInterface as Response;
use Psr\Http\Message\ServerRequestInterface as Request;
use Slim\Factory\AppFactory;
use OpenTelemetry\Contrib\Otlp\OtlpHttpTransportFactory;
use OpenTelemetry\Contrib\Otlp\SpanExporter;
use OpenTelemetry\SDK\Trace\TracerProvider;
use OpenTelemetry\SDK\Trace\SpanProcessor\SimpleSpanProcessor;

require __DIR__ . '/vendor/autoload.php';

if (!extension_loaded('ctype')) {
    error_log("ERROR: ctype PHP extension is NOT loaded. Exiting...");
    exit(255);
}

try {
    $app = AppFactory::create();
    error_log("INFO: Slim application instance created successfully.");
} catch (Throwable $e) {
    error_log("ERROR: Failed to create Slim application instance - " . $e->getMessage());
    exit(255);
}

$app->get('/test-tracer', function (Request $request, Response $response) {
    try {
        $otlpEndpoint = getenv('OTEL_EXPORTER_OTLP_TRACES_ENDPOINT');
        if (!$otlpEndpoint) {
            error_log("ERROR: OTEL_EXPORTER_OTLP_TRACES_ENDPOINT is not set.");
            return $response->withStatus(500)->write(json_encode(['error' => 'OTLP endpoint not configured.']));
        }

        $transport = (new OtlpHttpTransportFactory())->create($otlpEndpoint, 'application/x-protobuf');
        $exporter = new SpanExporter($transport);

        $tracerProvider = new TracerProvider(new SimpleSpanProcessor($exporter));
        $tracer = $tracerProvider->getTracer('io.opentelemetry.contrib.php');

        $span = $tracer->spanBuilder('test-span')->setSpanKind(\OpenTelemetry\API\Trace\SpanKind::KIND_INTERNAL)->startSpan();
        $span->setAttribute('example.attribute', 'test-value');

        // Exported span will contain:
        $spanData = [
            'traceId' => $span->getContext()->getTraceId(),
            'spanId' => $span->getContext()->getSpanId(),
            'name' => 'test-span',
            'kind' => 'INTERNAL',
            'attributes' => [
                'example.attribute' => 'test-value'
            ]
        ];
        error_log(json_encode($spanData));

        $span->end();
        $tracerProvider->shutdown();
    } catch (Throwable $e) {
        error_log("ERROR: " . $e->getMessage());
        return $response->withStatus(500)->write(json_encode(['error' => 'Tracing failed.']));
    }

    $response->getBody()->write(json_encode(['status' => 'Tracer tested and spans exported']));
    return $response->withHeader('Content-Type', 'application/json');
});

$app->get('/rolldice', function (Request $request, Response $response) {
    $result = random_int(1, 6);

    // Add logs for debugging
    error_log("INFO: Rolled a dice, result: $result");
    echo "DEBUG: Request handled at /rolldice\n";

    $response->getBody()->write(strval($result));
    return $response;
});

return $app;
