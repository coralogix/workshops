import logging
import json
from flask import Flask
import threading
import requests
import time

# Import OpenTelemetry packages
from opentelemetry import trace
from opentelemetry.instrumentation.flask import FlaskInstrumentor
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor
from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter
from opentelemetry.instrumentation.logging import LoggingInstrumentor

# Initialize Flask app
app = Flask(__name__)

# Setup OpenTelemetry
trace.set_tracer_provider(TracerProvider())
tracer_provider = trace.get_tracer_provider()

# Configure OTLP exporter
otlp_exporter = OTLPSpanExporter()
tracer_provider.add_span_processor(BatchSpanProcessor(otlp_exporter))

# Instrument Flask
FlaskInstrumentor().instrument_app(app)

# Enable OpenTelemetry logging instrumentation
LoggingInstrumentor().instrument(set_logging_format=True)

# Define a simple route
@app.route('/')
def home():
    return 'Hello, World from Flask!'

# Function to run the Flask server
def run_flask_app():
    app.run(debug=False, use_reloader=False, port=5000, host='127.0.0.1')

# Custom JSON formatter
class JsonFormatter(logging.Formatter):
    def format(self, record):
        log_record = {
            "time": self.formatTime(record, self.datefmt),
            "name": record.name,
            "level": record.levelname,
            "message": record.getMessage(),
            # Include trace context
            "trace_id": getattr(record, "otelTraceID", "N/A"),
            "span_id": getattr(record, "otelSpanID", "N/A"),
        }
        if record.exc_info:
            log_record["exception"] = self.formatException(record.exc_info)
        return json.dumps(log_record)

# Setup logging to output as JSON
def setup_logging():
    logger = logging.getLogger()
    logger.setLevel(logging.DEBUG)  # Set log level back to DEBUG
    
    ch = logging.StreamHandler()
    ch.setLevel(logging.DEBUG)  # Set handler log level back to DEBUG
    
    formatter = JsonFormatter()
    ch.setFormatter(formatter)
    
    logger.addHandler(ch)

# Main application code
if __name__ == '__main__':
    setup_logging()
    logger = logging.getLogger(__name__)

    flask_thread = threading.Thread(target=run_flask_app, daemon=True)
    flask_thread.start()

    logger.debug("Flask server started in background.")  # Ensure logs are at DEBUG level

    try:
        for i in range(200):
            response = requests.get("http://127.0.0.1:5000")
            logger.debug(f"Request {i+1}: Flask server responded with: {response.text}")  # Ensure logs are at DEBUG level
            time.sleep(0.1)
    except KeyboardInterrupt:
        logger.debug("Application stopped.")  # Ensure logs are at DEBUG level
