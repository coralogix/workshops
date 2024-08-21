from opentelemetry import metrics
from opentelemetry.sdk.metrics import MeterProvider
from opentelemetry.exporter.otlp.proto.grpc.metric_exporter import OTLPMetricExporter
from opentelemetry.sdk.metrics.export import PeriodicExportingMetricReader
from opentelemetry.sdk.resources import Resource

import time
import random

# Initialize the meter provider and the metric reader
resource = Resource.create(attributes={"service.name": "up_down_metric_app"})
exporter = OTLPMetricExporter(endpoint="http://localhost:4317", insecure=True)
reader = PeriodicExportingMetricReader(exporter, export_interval_millis=5000)
provider = MeterProvider(resource=resource, metric_readers=[reader])
metrics.set_meter_provider(provider)

# Create a meter instance
meter = metrics.get_meter(__name__)

# Create an up/down counter (gauge)
up_down_counter = meter.create_observable_gauge(
    name="up_down_metric",
    description="An up/down counter that can increase or decrease",
    callbacks=[lambda result: result.observe(random.randint(-5, 5))],
)

# Simulate metric changes in a loop
try:
    while True:
        # The actual value is updated in the callback.
        time.sleep(1)
except KeyboardInterrupt:
    print("Application stopped.")
