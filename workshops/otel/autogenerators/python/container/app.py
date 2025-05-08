import json
import logging
import threading
import time
import uuid
from typing import Any

import requests
from flask import Flask, request

# Initialize the Flask app
app = Flask(__name__)

@app.route('/')
def home() -> str:
    """
    Home route that returns a greeting and the request ID from headers.
    """
    request_id = request.headers.get('X-Request-ID', 'Unknown')
    return f'Hello, World from Flask! request_id: {request_id}'

def run_flask_app() -> None:
    """
    Runs the Flask app on a background thread.
    """
    app.run(debug=False, use_reloader=False, port=5000, host='0.0.0.0')

class JsonFormatter(logging.Formatter):
    """
    Custom JSON formatter for logging.
    """
    def format(self, record: logging.LogRecord) -> str:
        log_record = {
            "time": self.formatTime(record, self.datefmt),
            "name": record.name,
            "level": record.levelname,
            "body": record.getMessage(),
            "response_body": getattr(record, "responseBody", "N/A"),
            "trace_id": getattr(record, "otelTraceID", None),
            "span_id": getattr(record, "otelSpanID", None),
        }
        if record.exc_info:
            log_record["exception"] = self.formatException(record.exc_info)
        return json.dumps(log_record)

def setup_logging() -> None:
    """
    Configures logging to use the custom JSON formatter.
    """
    logger = logging.getLogger()
    logger.setLevel(logging.DEBUG)

    # Remove all handlers associated with the root logger object.
    if logger.hasHandlers():
        logger.handlers.clear()

    handler = logging.StreamHandler()
    handler.setLevel(logging.DEBUG)
    handler.setFormatter(JsonFormatter())
    logger.addHandler(handler)

def main() -> None:
    """
    Main function to start the Flask server and periodically make requests to it.
    """
    setup_logging()
    logger = logging.getLogger(__name__)

    # Start Flask server in a background thread
    flask_thread = threading.Thread(target=run_flask_app, daemon=True)
    flask_thread.start()
    logger.debug("Flask server started in background.")

    try:
        while True:
            request_id = str(uuid.uuid4())
            response = requests.get(
                "http://127.0.0.1:5000",
                headers={'X-Request-ID': request_id}
            )
            logger.debug(
                f"Received response: {response.text}",
                extra={'responseBody': response.text}
            )
            time.sleep(0.3)
    except KeyboardInterrupt:
        logger.debug("Application stopped.")

if __name__ == '__main__':
    main()