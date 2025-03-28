import logging
import json
from flask import Flask, request
import threading
import requests
import time
import uuid

# Initialize the Flask app
app = Flask(__name__)

# Define the home route
@app.route('/')
def home():
    request_id = request.headers.get('X-Request-ID', 'Unknown')
    return f'Hello, World from Flask! request_id: {request_id}'

# Run the Flask app in a background thread
def run_flask_app():
    app.run(debug=False, use_reloader=False, port=5000, host='0.0.0.0')

# Custom JSON formatter for logs
class JsonFormatter(logging.Formatter):
    def format(self, record):
        log_record = {
            "time": self.formatTime(record, self.datefmt),
            "name": record.name,
            "level": record.levelname,
            "body": record.getMessage(),
            "response_body": getattr(record, "responseBody", "N/A"),
        }
        if record.exc_info:
            log_record["exception"] = self.formatException(record.exc_info)
        return json.dumps(log_record)

# Configure logging with the custom JSON formatter
def setup_logging():
    logger = logging.getLogger()
    logger.setLevel(logging.DEBUG)
    logger.handlers.clear()  # Prevent duplicated handlers on reload

    handler = logging.StreamHandler()
    handler.setLevel(logging.DEBUG)

    formatter = JsonFormatter()
    handler.setFormatter(formatter)
    logger.addHandler(handler)

if __name__ == '__main__':
    setup_logging()
    logger = logging.getLogger(__name__)

    flask_thread = threading.Thread(target=run_flask_app, daemon=True)
    flask_thread.start()

    logger.debug("Flask server started in background.")

    try:
        while True:
            request_id = str(uuid.uuid4())
            response = requests.get("http://127.0.0.1:5000", headers={'X-Request-ID': request_id})
            logger.debug(f"Received response: {response.text}", extra={'responseBody': response.text})
            time.sleep(0.3)
    except KeyboardInterrupt:
        logger.debug("Application stopped.")
