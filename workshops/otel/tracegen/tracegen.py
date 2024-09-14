import os
import time
from opentelemetry import trace
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor, ConsoleSpanExporter
from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter

# Set the OTEL service name
os.environ["OTEL_SERVICE_NAME"] = "tracegen"

# Set up the tracer provider and exporter
provider = TracerProvider()

# OTLP exporter for sending traces to OTEL collector
otlp_exporter = OTLPSpanExporter(endpoint="http://localhost:4317", insecure=True)

# Console exporter for printing traces to the console
console_exporter = ConsoleSpanExporter()

# Set up batch processors for both exporters
provider.add_span_processor(BatchSpanProcessor(otlp_exporter))
provider.add_span_processor(BatchSpanProcessor(console_exporter))

trace.set_tracer_provider(provider)

# Create a tracer
tracer = trace.get_tracer(__name__)

# Infinite loop to keep creating spans
while True:
    with tracer.start_as_current_span("example-trace") as span:
        trace_id = span.get_span_context().trace_id
        span_id = span.get_span_context().span_id
        # Print the trace and span IDs in the message
        print(f"Created span with Trace ID: {format(trace_id, '032x')}, Span ID: {format(span_id, '016x')}")
    # Sleep for a short time to avoid spamming
    time.sleep(1)
