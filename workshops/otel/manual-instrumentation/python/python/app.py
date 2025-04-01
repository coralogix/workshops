import random
import time
from opentelemetry import metrics
from opentelemetry.sdk.metrics import MeterProvider
from opentelemetry.exporter.otlp.proto.grpc.metric_exporter import OTLPMetricExporter
from opentelemetry.sdk.metrics.export import PeriodicExportingMetricReader

# Setup MeterProvider with OTLP exporter
exporter = OTLPMetricExporter(endpoint="http://localhost:4317", insecure=True)
reader = PeriodicExportingMetricReader(exporter, export_interval_millis=1000)
provider = MeterProvider(metric_readers=[reader])
metrics.set_meter_provider(provider)

# Create a meter
meter = metrics.get_meter(__name__)

# Create an UpDownCounter to simulate a gauge-like behavior
random_number_gauge = meter.create_up_down_counter(
    name="latest_random_number",
    description="Tracks the last generated random number",
    unit="1"
)

# Function to simulate random number generation and update the gauge
def generate_random_number():
    global latest_random_number
    while True:
        random_number = random.randint(0, 100)
        random_number_gauge.add(random_number - latest_random_number)
        latest_random_number = random_number
        print(f"latest_random_number:{latest_random_number}")
        time.sleep(1)

if __name__ == "__main__":
    latest_random_number = 0  # Initialize the latest random number
    generate_random_number()