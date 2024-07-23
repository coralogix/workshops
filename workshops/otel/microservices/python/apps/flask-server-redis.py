from flask import Flask, request, jsonify
import logging
import os
import datetime
import redis
from random import uniform, random
import uuid
import ipaddress
import time
import json
from opentelemetry import trace
from opentelemetry.instrumentation.flask import FlaskInstrumentor
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor, ConsoleSpanExporter

# Define the Flask app
app = Flask(__name__)

# Setup OpenTelemetry Tracing
trace.set_tracer_provider(TracerProvider())
tracer = trace.get_tracer(__name__)
span_processor = BatchSpanProcessor(ConsoleSpanExporter())
trace.get_tracer_provider().add_span_processor(span_processor)
FlaskInstrumentor().instrument_app(app)

# Configure logging to output JSON, customizing for all error types
class CustomFormatter(logging.Formatter):
    def format(self, record):
        base_log_record = {
            "time": datetime.datetime.utcnow().isoformat() + "Z",
            "severity": "ERROR" if record.levelno >= logging.ERROR else record.levelname,
            "message": record.getMessage()
        }
        response_data = json.loads(record.response_data) if hasattr(record, "response_data") else {}
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
redis_password = os.getenv('REDIS_PASSWORD', '')
r = redis.Redis(host=redis_host, port=redis_port, password=redis_password, decode_responses=True)

# Check the environment variable to determine the response behavior
PYTHON_TEST_URLGOOD = os.getenv('PYTHON_TEST_URLGOOD', 'GOOD')

@app.route("/<path:path>")
def handle_request(path):
    try:
        if PYTHON_TEST_URLGOOD == "BAD":
            if random() < 0.80:  # 80% chance to decide behavior
                # Either perform a slow response or raise a 500 error
                if random() < 0.5:
                    # Simulate slow response
                    time.sleep(uniform(5, 10))  # Increased delay
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
        logging.info("Request handled successfully", extra={"response_data": json.dumps(response_data)})
        return jsonify(response_data), 200
    except Exception as e:
        logging.error("Internal Server Error", extra={"response_data": json.dumps({"error": str(e)})})
        return jsonify(error="Internal Server Error"), 500

if __name__ == '__main__':
    app.run(host='0.0.0.0')