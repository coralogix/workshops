import logging
import json
from flask import Flask, request
import threading
import requests
import time
import uuid
import os
from logging.handlers import RotatingFileHandler

from opentelemetry import trace
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.instrumentation.logging import LoggingInstrumentor

app = Flask(__name__)

# Set up the tracer provider only if it hasn't been set already
if not trace.get_tracer_provider():
    trace.set_tracer_provider(TracerProvider())
tracer_provider = trace.get_tracer_provider()

# Enable logging instrumentation to attach trace context to logs
LoggingInstrumentor().instrument(set_logging_format=True)

@app.route('/')
def home():
    request_id = request.headers.get('X-Request-ID', 'Unknown')
    response_message = f'Hello, World from Flask! request_id: {request_id}'
    return response_message

def run_flask_app():
    app.run(debug=False, use_reloader=False, port=5000, host='127.0.0.1')

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
    
    # Stream handler for console output
    ch = logging.StreamHandler()
    ch.setLevel(logging.DEBUG)
    
    formatter = JsonFormatter()
    ch.setFormatter(formatter)
    logger.addHandler(ch)
    
    # Rotating file handler for file output
    log_file = "/tmp/cx-autogen-python.log"
    fh = RotatingFileHandler(
        log_file, 
        maxBytes=10*1024*1024,  # 10 MB
        backupCount=5
    )
    fh.setLevel(logging.DEBUG)
    fh.setFormatter(formatter)
    logger.addHandler(fh)

if __name__ == '__main__':
    setup_logging()
    logger = logging.getLogger(__name__)

    flask_thread = threading.Thread(target=run_flask_app, daemon=True)
    flask_thread.start()

    logger.debug("Flask server started in background.")

    # Wait until the Flask server is up
    time.sleep(1)

    try:
        while True:
            request_id = str(uuid.uuid4())
            response = requests.get("http://127.0.0.1:5000", headers={'X-Request-ID': request_id})
            logger.debug(f"Received response: {response.text}", extra={'responseBody': response.text})
            time.sleep(0.3)
    except KeyboardInterrupt:
        logger.debug("Application stopped.")
