import os
import sys
import json
import logging
import requests
from time import sleep
from random import random, seed
from opentelemetry import trace
from opentelemetry.instrumentation.requests import RequestsInstrumentor
from opentelemetry.instrumentation.logging import LoggingInstrumentor
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor, ConsoleSpanExporter

# Seed the random number generator for reproducibility
seed()

# Initialize OpenTelemetry for tracing and logging
trace.set_tracer_provider(TracerProvider())
tracer = trace.get_tracer(__name__)
span_processor = BatchSpanProcessor(ConsoleSpanExporter())
trace.get_tracer_provider().add_span_processor(span_processor)
RequestsInstrumentor().instrument()
LoggingInstrumentor().instrument(set_logging_format=True)

# Set up logging to output JSON with trace information
class CustomFormatter(logging.Formatter):
    def format(self, record):
        span = trace.get_current_span()
        trace_id = span.get_span_context().trace_id
        span_id = span.get_span_context().span_id
        log_record = {
            "time": logging.Formatter.formatTime(self, record, datefmt="%Y-%m-%dT%H:%M:%S"),
            "span_id": format(span_id, "016x"),
            "trace_id": format(trace_id, "032x"),
            "service_name": getattr(record, "otelServiceName", "PythonService"),
        }

        if record.levelno >= logging.ERROR:
            log_record["severity"] = "ERROR"
            log_record["error"] = record.getMessage()
        else:
            log_record["severity"] = record.levelname
            log_record["message"] = record.getMessage()

        if hasattr(record, 'extra'):
            log_record.update(record.extra)  # Include additional data if available

        return json.dumps(log_record)

# Configure logger with handlers and set level
logger = logging.getLogger()
handler = logging.StreamHandler()
handler.setFormatter(CustomFormatter())
logger.addHandler(handler)
logger.setLevel(logging.DEBUG)

# Environment variables
env_url = os.getenv('PYTHON_TEST_URL', 'http://example.com')
env_url_good = os.getenv('PYTHON_TEST_URLGOOD', 'GOOD')

def python_reqs():
    try:
        # Determine URL based on environment settings
        bad_chance = random()
        if env_url_good == "BAD" and bad_chance <= 0.85:
            url = env_url
        else:
            url = f"{env_url}/transact"

        with tracer.start_as_current_span("python_reqs") as span:
            response = requests.get(url)
            response_data = response.json() if response.headers.get('content-type') == 'application/json' else response.text

            if response.status_code != 200:
                logging.error("HTTP error received", extra={"response": response_data, "status_code": response.status_code})
            else:
                logging.info("Response received", extra={"response": response_data})

    except requests.exceptions.RequestException as err:
        logging.error("Request failed due to network problem", extra={"error": str(err)})
    except Exception as e:
        logging.error("An unexpected error occurred", extra={"error": str(e)})

if __name__ == "__main__":
    while True:
        python_reqs()
        sleep(round(random(), 1))
