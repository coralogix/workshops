import logging
import json
import requests
from flask import Flask, jsonify, request
from threading import Thread
import time

# Create a Flask application
app = Flask(__name__)

@app.route('/self', methods=['GET'])
def self_endpoint():
    return jsonify({"message": "Hello from Flask!"})

@app.route('/shutdown', methods=['GET'])
def shutdown():
    func = request.environ.get('werkzeug.server.shutdown')
    if func is None:
        raise RuntimeError('Not running with the Werkzeug Server')
    func()
    return 'Server shutting down...'

# Custom JSON formatter for logging
class JsonFormatter(logging.Formatter):
    def format(self, record):
        log_record = {
            "time": self.formatTime(record, self.datefmt),
            "name": record.name,
            "level": record.levelname,
            "message": record.getMessage(),
            "status_code": getattr(record, "statusCode", "N/A"),
            "url": getattr(record, "url", "N/A"),
        }
        if record.exc_info:
            log_record["exception"] = self.formatException(record.exc_info)
        return json.dumps(log_record)

# Set up logging with JSON format
def setup_logging():
    logger = logging.getLogger()
    logger.setLevel(logging.DEBUG)
    ch = logging.StreamHandler()
    ch.setLevel(logging.DEBUG)
    formatter = JsonFormatter()
    ch.setFormatter(formatter)
    logger.addHandler(ch)

# Function to make a request to the Flask server using Flask's test client
def make_self_request():
    with app.test_client() as client:
        response = client.get('/self')
        return response

# Lambda handler function
def lambda_handler(event, context):
    setup_logging()
    logger = logging.getLogger(__name__)
    
    try:
        for _ in range(10):
            response = make_self_request()
            response_data = response.get_json()
            log_data = {"statusCode": response.status_code, "url": "/self", "message": response_data['message']}
            logger.info(
                "Successfully made a self-request",
                extra={"statusCode": response.status_code, "url": "/self"}
            )
            print(json.dumps(log_data, indent=2))
        
        return {
            'statusCode': 200,
            'body': "Successfully logged 10 requests."
        }
    except Exception as e:
        logger.error("Error making self-requests", exc_info=True)
        return {
            'statusCode': 500,
            'body': json.dumps({'error': str(e)})
        }

if __name__ == '__main__':
    # Configure logging
    setup_logging()
    logger = logging.getLogger(__name__)

    try:
        for _ in range(10):
            response = make_self_request()
            response_data = response.get_json()
            log_data = {"statusCode": response.status_code, "url": "/self", "message": response_data['message']}
            logger.info(
                "Successfully made a self-request",
                extra={"statusCode": response.status_code, "url": "/self"}
            )
            print(json.dumps(log_data, indent=2))
    except Exception as e:
        logger.error("Error making self-requests", exc_info=True)
        print({'error': str(e)})
