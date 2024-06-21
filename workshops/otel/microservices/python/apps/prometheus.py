import os
from prometheus_client import start_http_server, Summary, Gauge
import random
import time

# Get the gauge name from an environment variable
gauge_name = os.getenv('GAUGE_NAME')
if not gauge_name:
    raise ValueError('Environment variable GAUGE_NAME must be set.')

# Create a metric to track time spent and requests made.
REQUEST_TIME = Summary('request_processing_seconds', 'Time spent processing request')

# Set Up Gauge with a name obtained from the environment variable
g = Gauge(gauge_name, 'Example Custom Gauge')

def get_smoothed_value(current_value, alpha=0.1):
    """
    Generate a smoothed, integer value for the gauge.

    This function calculates a new gauge value by taking a weighted average of the current gauge value
    and a new random value, then rounding to the nearest integer.

    Args:
    current_value (float): The current value of the gauge.
    alpha (float): The smoothing factor, controlling the mix of the current and new values.

    Returns:
    int: The new, smoothed, and rounded value for the gauge.
    """
    new_value = alpha * random.randint(1, 100) + (1 - alpha) * current_value
    return int(round(new_value))

@REQUEST_TIME.time()
def process_request(t, last_value):
    """
    Simulate processing a request and update the gauge with a smoothed, integer value.

    Args:
    t (float): The amount of time in seconds to simulate processing a request.
    last_value (float): The last value of the gauge.
    
    Returns:
    int: The updated, integer gauge value.
    """
    time.sleep(t)
    new_value = get_smoothed_value(last_value)
    g.set(new_value)
    return new_value

if __name__ == '__main__':
    # Start up the server to expose the metrics.
    start_http_server(9090)
    print(f'Starting Prometheus metrics server on port 9090 with gauge {gauge_name}')
    
    # Generate some requests with smoothed gauge values.
    last_value = 50  # Initialize with a middle value for the gauge
    while True:
        last_value = process_request(random.random(), last_value)
