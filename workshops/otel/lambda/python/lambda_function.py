import logging
import json
from flask import Flask, request
from serverless_wsgi import handle_request

# Initialize Flask app
app = Flask(__name__)

@app.route('/')
def home():
    request_id = request.headers.get('X-Request-ID', 'Unknown')
    return f'Hello, World from Flask! request_id: {request_id}'

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

def setup_logging():
    logger = logging.getLogger()
    if not logger.handlers:
        logger.setLevel(logging.DEBUG)
        ch = logging.StreamHandler()
        ch.setLevel(logging.DEBUG)
        ch.setFormatter(JsonFormatter())
        logger.addHandler(ch)

# Configure logging globally
setup_logging()

def lambda_handler(event, context):
    return handle_request(app, event, context)

if __name__ == '__main__':
    app.run(debug=True, port=5000)
