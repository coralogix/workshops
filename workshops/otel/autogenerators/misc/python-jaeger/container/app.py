import logging
import json
import os
import threading
import requests
import time
import uuid
from flask import Flask, request
from jaeger_client import Config
import opentracing
from opentracing.ext import tags

# Initialize Flask app
app = Flask(__name__)

# Set up logging
class JsonFormatter(logging.Formatter):
    def format(self, record):
        log_record = {
            "time": self.formatTime(record, self.datefmt),
            "name": record.name,
            "level": record.levelname,
            "body": record.getMessage(),
            "trace_id": getattr(record, "trace_id", "N/A"),
            "span_id": getattr(record, "span_id", "N/A"),
            "response_body": getattr(record, "responseBody", "N/A"),
        }
        if record.exc_info:
            log_record["exception"] = self.formatException(record.exc_info)
        return json.dumps(log_record)

def setup_logging():
    logger = logging.getLogger()
    logger.setLevel(logging.DEBUG)
    handler = logging.StreamHandler()
    handler.setLevel(logging.DEBUG)
    handler.setFormatter(JsonFormatter())
    logger.addHandler(handler)

logger = logging.getLogger(__name__)

# Initialize Jaeger tracer
def init_tracer(service_name):
    config = Config(
        config={
            'sampler': {'type': 'const', 'param': 1},
            'logging': True,
            'reporter_batch_size': 1,
            'local_agent': {
                'reporting_host': os.getenv('OTEL_EXPORTER_JAEGER_AGENT_HOST', 'localhost'),
                'reporting_port': int(os.getenv('OTEL_EXPORTER_JAEGER_AGENT_PORT', '6831')),
            },
        },
        service_name=service_name,
        validate=True,
    )
    return config.initialize_tracer()

tracer = init_tracer(os.getenv("OTEL_SERVICE_NAME", "native-jaeger-app"))
opentracing.tracer = tracer

@app.route('/')
def home():
    request_id = request.headers.get('X-Request-ID', 'Unknown')
    with tracer.start_active_span("handle-home-request") as scope:
        span = scope.span
        span.set_tag("request.id", request_id)
        span.set_tag(tags.SPAN_KIND, tags.SPAN_KIND_RPC_SERVER)

        # Add trace/span IDs to logger for this request
        trace_id = format(span.context.trace_id, "x")
        span_id = format(span.context.span_id, "x")
        logger.debug(f"Received request", extra={"trace_id": trace_id, "span_id": span_id})

        return f'Hello, World from Flask! request_id: {request_id}'

def run_flask_app():
    app.run(debug=False, use_reloader=False, port=5000, host='0.0.0.0')

if __name__ == '__main__':
    setup_logging()
    logger.debug("Flask server starting...")
    flask_thread = threading.Thread(target=run_flask_app, daemon=True)
    flask_thread.start()

    try:
        while True:
            request_id = str(uuid.uuid4())
            response = requests.get("http://127.0.0.1:5000", headers={'X-Request-ID': request_id})
            logger.debug(f"Received response: {response.text}", extra={'responseBody': response.text})
            time.sleep(0.3)
    except KeyboardInterrupt:
        logger.debug("Application stopped.")
        tracer.close()
