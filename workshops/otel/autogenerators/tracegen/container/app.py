import os
import time
from opentelemetry import trace
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor
from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter

# Set the OTEL service name
os.environ["OTEL_SERVICE_NAME"] = "tracegen"

# Get OTLP endpoint from environment variable, or default to localhost:4317
otlp_endpoint = os.getenv("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4317")

# Set up the tracer provider and OTLP exporter
provider = TracerProvider()
otlp_exporter = OTLPSpanExporter(endpoint=otlp_endpoint, insecure=True)

# Set up batch processor for the OTLP exporter
provider.add_span_processor(BatchSpanProcessor(otlp_exporter))

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
