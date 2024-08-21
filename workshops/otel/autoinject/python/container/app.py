# Import necessary libraries for web server, logging, threading, HTTP requests, and unique ID generation
import logging
import json
from flask import Flask, request
import threading
import requests
import time
import uuid

# Initialize the Flask application
app = Flask(__name__)

# Define the home route
@app.route('/')
def home():
    # Retrieve the request ID from the incoming request headers
    request_id = request.headers.get('X-Request-ID', 'Unknown')
    # Respond with a message that includes the request ID
    response_message = f'Hello, World from Flask! request_id: {request_id}'
    return response_message

# Function to run the Flask app in a separate thread
def run_flask_app():
    # Start the Flask application without debug mode and reloader
    app.run(debug=False, use_reloader=False, port=5000, host='127.0.0.1')

# Custom JSON formatter for logging
class JsonFormatter(logging.Formatter):
    def format(self, record):
        # Construct a JSON log record using standard and custom fields
        log_record = {
            "time": self.formatTime(record, self.datefmt),
            "name": record.name,
            "level": record.levelname,
            "body": record.getMessage(),
            "request_id": getattr(record, "requestID", "N/A"),
            "response_body": getattr(record, "responseBody", "N/A"),
        }
        if record.exc_info:
            log_record["exception"] = self.formatException(record.exc_info)
        return json.dumps(log_record)

# Set up logging with JSON format
def setup_logging():
    # Get the root logger
    logger = logging.getLogger()
    # Set logging level to DEBUG
    logger.setLevel(logging.DEBUG)
    
    # Create a stream handler for outputting logs
    ch = logging.StreamHandler()
    ch.setLevel(logging.DEBUG)
    
    # Use the custom JSON formatter
    formatter = JsonFormatter()
    ch.setFormatter(formatter)
    
    # Add the handler to the logger
    logger.addHandler(ch)

if __name__ == '__main__':
    # Configure logging
    setup_logging()
    # Get a logger instance for this module
    logger = logging.getLogger(__name__)

    # Start the Flask app in a background thread
    flask_thread = threading.Thread(target=run_flask_app, daemon=True)
    flask_thread.start()

    # Log that the Flask server has started
    logger.debug("Flask server started in background.")

    try:
        # Main loop to send requests to the Flask server
        while True:
            # Generate a unique request ID for each request
            request_id = str(uuid.uuid4())
            # Send a GET request to the Flask server with the unique request ID
            response = requests.get("http://127.0.0.1:5000", headers={'X-Request-ID': request_id})
            # Log the response using the custom logger, including the response text and request ID
            logger.debug(
                f"Received response: {response.text}", 
                extra={'responseBody': response.text, 'requestID': request_id}
            )
            # Wait for a short period before sending the next request
            time.sleep(0.3)
    except KeyboardInterrupt:
        # Log a message when the script is stopped manually
        logger.debug("Application stopped.")
