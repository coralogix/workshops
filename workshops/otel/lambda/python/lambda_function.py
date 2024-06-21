import logging
import json
from flask import Flask, request
from serverless_wsgi import handle_request
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

# Custom JSON formatter for logging
class JsonFormatter(logging.Formatter):
    def format(self, record):
        # Construct a JSON log record using standard and custom fields
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

# Lambda handler function
def lambda_handler(event, context):
    setup_logging()
    return handle_request(app, event, context)

if __name__ == '__main__':
    # Configure logging
    setup_logging()
    # Get a logger instance for this module
    logger = logging.getLogger(__name__)

    # Start the Flask app (for local testing)
    app.run(debug=True, port=5000)