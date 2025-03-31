from opentelemetry import metrics
from opentelemetry.sdk.metrics import MeterProvider
from opentelemetry.sdk.resources import Resource
from opentelemetry.sdk.metrics.export import PeriodicExportingMetricReader
from opentelemetry.exporter.otlp.proto.grpc.metric_exporter import OTLPMetricExporter

import time
import random

# Define the resource (e.g., service name)
resource = Resource(attributes={
    "service.name": "exp-histogram-example"
})

# Set up the OTLP exporter
exporter = OTLPMetricExporter(endpoint="localhost:4317", insecure=True)

# Set up the reader to periodically export metrics
reader = PeriodicExportingMetricReader(exporter, export_interval_millis=5000)

# Configure the MeterProvider
provider = MeterProvider(resource=resource, metric_readers=[reader])
metrics.set_meter_provider(provider)

# Get a Meter
meter = metrics.get_meter("exp.histogram.meter")

# Create an exponential histogram (asynchronous or synchronous)
# We'll use a synchronous observable for example purposes
histogram = meter.create_histogram(
    name="request.latency",
    unit="ms",
    description="Exponential histogram of request latencies"
)

# Simulate measurements
for _ in range(20):
    latency = random.expovariate(1 / 50)  # e.g., average latency ~50ms
    histogram.record(latency, attributes={"endpoint": "/api/data"})
    print(f"metric request.latency: {latency:.2f} ms")
    time.sleep(1)