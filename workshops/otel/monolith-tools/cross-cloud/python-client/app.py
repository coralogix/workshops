import logging
import json
import requests
import time
import uuid
from pathlib import Path
from logging.handlers import RotatingFileHandler

from opentelemetry import trace
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.instrumentation.logging import LoggingInstrumentor

# Set up the tracer provider if not already set
if not trace.get_tracer_provider():
    trace.set_tracer_provider(TracerProvider())

# Enable logging instrumentation to attach trace context to logs
LoggingInstrumentor().instrument(set_logging_format=True)

class JsonFormatter(logging.Formatter):
    def format(self, record):
        log_record = {
            "time": self.formatTime(record, self.datefmt),
            "name": record.name,
            "level": record.levelname,
            "body": record.getMessage(),
            "trace_id": getattr(record, "otelTraceID", "N/A"),
            "span_id": getattr(record, "otelSpanID", "N/A"),
            "response_body": getattr(record, "responseBody", "N/A"),
        }
        if record.exc_info:
            log_record["exception"] = self.formatException(record.exc_info)
        return json.dumps(log_record)

def setup_logging():
    logger = logging.getLogger()
    logger.setLevel(logging.DEBUG)

    # Create log directory if it doesn't exist
    log_dir = Path("/var/log/cx")
    log_dir.mkdir(parents=True, exist_ok=True)

    # File handler for logging to /var/log/cx/application.log
    log_file = log_dir / "application.log"
    fh = RotatingFileHandler(
        log_file,
        maxBytes=10 * 1024 * 1024,  # 10 MB
        backupCount=5
    )
    fh.setLevel(logging.DEBUG)
    fh.setFormatter(JsonFormatter())
    logger.addHandler(fh)

    # Stream handler for console output
    ch = logging.StreamHandler()
    ch.setLevel(logging.DEBUG)
    ch.setFormatter(JsonFormatter())
    logger.addHandler(ch)

if __name__ == '__main__':
    # Set up logging
    setup_logging()
    logger = logging.getLogger(__name__)

    logger.debug("Starting client application.")

    # Define the Java app server endpoint
    SERVER_URL = "http://ec2-54-188-235-183.us-west-2.compute.amazonaws.com:8080/api/data"

    try:
        while True:
            # Generate a UUID for the request
            request_id = str(uuid.uuid4())

            # Log the request attempt
            logger.debug(f"Sending request to {SERVER_URL} with UUID: {request_id}")

            # Send the GET request with the UUID header
            response = requests.get(SERVER_URL, headers={'UUID': request_id})

            # Log the received response
            logger.debug(
                f"Received response",
                extra={
                    'responseBody': response.text
                }
            )

            # Delay before sending the next request
            time.sleep(0.3)
    except KeyboardInterrupt:
        logger.debug("Application stopped.")