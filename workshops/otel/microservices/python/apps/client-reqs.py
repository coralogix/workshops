import os
import sys
import json
import logging
import asyncio
import httpx
from random import random, seed
from opentelemetry import trace
from opentelemetry.instrumentation.logging import LoggingInstrumentor
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor, ConsoleSpanExporter
from tenacity import retry, stop_after_attempt, wait_exponential_jitter
import pybreaker

# Seed the random number generator for reproducibility
seed()

# Initialize OpenTelemetry for tracing and logging
trace.set_tracer_provider(TracerProvider())
tracer = trace.get_tracer(__name__)
span_processor = BatchSpanProcessor(ConsoleSpanExporter())
trace.get_tracer_provider().add_span_processor(span_processor)
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
env_url = os.getenv('PYTHON_TEST_URL', 'http://localhost:8000')
env_url_good = os.getenv('PYTHON_TEST_URLGOOD', 'GOOD')

# Circuit breaker configuration
breaker = pybreaker.CircuitBreaker(
    fail_max=5,  # Number of failures before opening the circuit
    reset_timeout=30  # Time in seconds to reset the circuit
)

@retry(stop=stop_after_attempt(5), wait=wait_exponential_jitter(initial=1, max=10))
async def fetch_with_retry(client, url):
    """Fetch the URL with retry logic."""
    response = await client.get(url, timeout=5.0)
    response.raise_for_status()  # Raise an error for non-2xx status codes
    return response

@breaker
async def python_reqs():
    """Perform the HTTP request with retry and circuit breaker logic."""
    try:
        # Determine URL based on environment settings
        bad_chance = random()
        url = env_url if env_url_good == "BAD" and bad_chance <= 0.85 else f"{env_url}/transact"

        async with httpx.AsyncClient() as client:
            with tracer.start_as_current_span("python_reqs") as span:
                response = await fetch_with_retry(client, url)
                response_data = (
                    response.json()
                    if response.headers.get('content-type') == 'application/json'
                    else response.text
                )

                logging.info("Response received", extra={"response": response_data})

    except pybreaker.CircuitBreakerError:
        logging.error("Circuit breaker is open. Skipping request.")
    except httpx.RequestError as err:
        logging.error("Request failed due to network problem", extra={"error": str(err)})
    except Exception as e:
        logging.error("An unexpected error occurred", extra={"error": str(e)})

if __name__ == "__main__":
    async def main():
        while True:
            await python_reqs()
            await asyncio.sleep(round(random(), 1))

    asyncio.run(main())