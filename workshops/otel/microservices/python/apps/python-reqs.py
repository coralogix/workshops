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

# Seed the random number generator for reproducibility
seed()

# Initialize OpenTelemetry for tracing and logging
trace.set_tracer_provider(TracerProvider())
tracer = trace.get_tracer(__name__)
RequestsInstrumentor().instrument()
LoggingInstrumentor().instrument(set_logging_format=True)

# Set up logging to output JSON with trace information
class CustomFormatter(logging.Formatter):
    def format(self, record):
        log_record = {
            "time": logging.Formatter.formatTime(self, record, datefmt="%Y-%m-%dT%H:%M:%S"),
            "span_id": getattr(record, "otelSpanID", "0000000000000000"),
            "trace_id": getattr(record, "otelTraceID", "00000000000000000000000000000000"),
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
env_url = os.getenv('PYTHON_TEST_URL')
env_url_good = os.getenv('PYTHON_TEST_URLGOOD')
one_shot = os.getenv('PYTHON_ONESHOT')

def python_reqs():
    try:
        # Determine URL based on environment settings
        bad_chance = random()
        if one_shot == "NO" and env_url_good == "BAD" and bad_chance <= 0.85:
            url = env_url
        else:
            url = env_url + "/transact" if env_url else "http://example.com/transact"

        response = requests.get(url)
        if response.status_code != 200:
            logging.error("HTTP error received", extra={"response": response.json(), "status_code": response.status_code})
        else:
            logging.info("Response received", extra={"response": response.json()})

        if one_shot == "YES":
            sys.exit("Oneshot")

    except requests.exceptions.RequestException as err:
        logging.error("Request failed due to network problem", extra={"response": str(err)})

while True:
    python_reqs()
    sleep(round(random(), 1))
