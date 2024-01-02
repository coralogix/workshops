const http = require('http');
const opentelemetry = require('@opentelemetry/api');
const { NodeTracerProvider } = require('@opentelemetry/sdk-trace-node');
const { SimpleSpanProcessor, ConsoleSpanExporter } = require('@opentelemetry/sdk-trace-base');
const { HttpInstrumentation } = require('@opentelemetry/instrumentation-http');

// Set up OpenTelemetry tracer
const provider = new NodeTracerProvider();
provider.addSpanProcessor(new SimpleSpanProcessor(new ConsoleSpanExporter()));
provider.register();

// Enable HTTP instrumentation
const httpInstrumentation = new HttpInstrumentation();
httpInstrumentation.setTracerProvider(provider);

function httpget() {
    const currentSpan = opentelemetry.trace.getSpan(opentelemetry.context.active());
    const traceId = currentSpan.context().traceId;
    const spanId = currentSpan.context().spanId;

    console.log(`Trace ID: ${traceId}, Span ID: ${spanId}`);

    const options = {
        hostname: 'api.github.com',
        path: '/',
        method: 'GET',
        headers: {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36'
        }
    };

    const req = http.request(options, (res) => {
        console.log(`statusCode: ${res.statusCode}`);
        console.log(`headers: ${JSON.stringify(res.headers)}`);

        res.on('data', (d) => {
            process.stdout.write(d);
        });
    });

    req.on('error', (error) => {
        console.error(error);
    });

    req.end();
}

const interval = 750;

for (let i = 0; i <= 100000; i++) {
    setTimeout(function(i) {
        httpget();
    }, interval * i, i);
}