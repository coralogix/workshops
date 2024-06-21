from flask import Flask, request, abort, jsonify
import logging
import os
import datetime
import redis
from random import uniform, random
import uuid
import ipaddress
from opentelemetry import trace
from opentelemetry.instrumentation.flask import FlaskInstrumentor
from opentelemetry.instrumentation.logging import LoggingInstrumentor
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.trace.status import Status, StatusCode
import time
import json

# Initialize OpenTelemetry for logging purposes
trace.set_tracer_provider(TracerProvider())
tracer = trace.get_tracer(__name__)

# Instrument Flask app with OpenTelemetry for tracing
app = Flask(__name__)
FlaskInstrumentor().instrument_app(app)
LoggingInstrumentor().instrument(set_logging_format=True)

# Configure logging to output JSON, customizing for all error types
class CustomFormatter(logging.Formatter):
    def format(self, record):
        base_log_record = {
            "time": datetime.datetime.utcnow().isoformat() + "Z",
            "span_id": getattr(record, "otelSpanID", "0000000000000000"),
            "trace_id": getattr(record, "otelTraceID", "00000000000000000000000000000000"),
            "severity": "ERROR" if record.levelno >= logging.ERROR else record.levelname,
            "message": record.getMessage()
        }
        response_data = getattr(record, "response_data", {})
        base_log_record.update(response_data)
        return json.dumps(base_log_record)

# Set the custom formatter for the root logger
logger = logging.getLogger()
logger.setLevel(logging.INFO)
handler = logging.StreamHandler()
handler.setFormatter(CustomFormatter())
logger.addHandler(handler)

# Redis Setup
redis_host = os.getenv('REDIS_SERVICE_HOST', 'localhost')
redis_port = 6379
redis_password = ""
r = redis.Redis(host=redis_host, port=redis_port, password=redis_password, decode_responses=True)

# Check the environment variable to determine the response behavior
PYTHON_TEST_URLGOOD = os.getenv('PYTHON_TEST_URLGOOD', 'GOOD')

@app.route("/<path:path>")
def handle_request(path):
    with tracer.start_as_current_span("handle_request") as span:
        if PYTHON_TEST_URLGOOD == "BAD":
            if random() < 0.50:  # 50% chance to decide behavior
                # Either perform a slow response or raise a 500 error
                if random() < 0.5:
                    # Simulate slow response
                    time.sleep(uniform(3, 7))
                else:
                    # Raise 500 error
                    raise Exception("Simulated Internal Server Error due to BAD configuration")
        
        transaction_id = str(uuid.uuid4())
        r.set(transaction_id, "transaction success")
        r.expire(transaction_id, 3600)

        network = ipaddress.ip_network('10.0.0.0/8')
        random_ip = ipaddress.ip_address(network.network_address + int(uniform(1, network.num_addresses - 1)))

        response_data = {
            "USER_IP": str(random_ip),
            "transaction": transaction_id,
            "result": path,
            "datetime": datetime.datetime.now().isoformat()
        }
        logging.info("Request handled successfully", extra={"response_data": response_data})
        return jsonify(response_data), 200

@app.errorhandler(500)
def handle_500_error(e):
    return jsonify(error="Internal Server Error"), 500

if __name__ == '__main__':
    app.run(host='0.0.0.0')