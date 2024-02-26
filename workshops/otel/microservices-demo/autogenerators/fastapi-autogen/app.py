import logging
import json
import uuid
from fastapi import FastAPI, Request, BackgroundTasks
import requests
import asyncio

# Initialize the FastAPI application
app = FastAPI()

# Custom JSON formatter for structured logging
class JsonFormatter(logging.Formatter):
    def format(self, record):
        # Create a structured log record as JSON
        log_record = {
            "time": self.formatTime(record, self.datefmt),
            "name": record.name,
            "level": record.levelname,
            "body": record.getMessage(),
            "trace_id": getattr(record, "otelTraceID", "N/A"),
            "span_id": getattr(record, "otelSpanID", "N/A"),
            "response_body": getattr(record, "responseBody", "N/A"),
        }
        # Add exception info if present
        if record.exc_info:
            log_record["exception"] = self.formatException(record.exc_info)
        return json.dumps(log_record)

# Configure logging with the custom JSON formatter
def setup_logging():
    logger = logging.getLogger()
    logger.setLevel(logging.DEBUG)  # Set global log level
    
    ch = logging.StreamHandler()
    ch.setLevel(logging.DEBUG)  # Set handler log level
    
    formatter = JsonFormatter()  # Use custom formatter
    ch.setFormatter(formatter)
    
    logger.addHandler(ch)  # Add handler to the logger

setup_logging()  # Initialize logging setup
logger = logging.getLogger(__name__)  # Get a logger for this module

# Root path route handler
@app.get('/')
async def home(request: Request):
    request_id = request.headers.get('X-Request-ID', str(uuid.uuid4()))  # Get or generate request ID
    response_message = f'Hello, World from FastAPI! request_id: {request_id}'
    return {"message": response_message}  # Return a JSON response

# Endpoint to start looping requests in the background
@app.post('/start-looping-requests/')
async def start_looping_requests(background_tasks: BackgroundTasks):
    background_tasks.add_task(looping_requests)  # Add the looping task to the background
    return {"message": "Started looping requests in the background"}

# Function to send requests to the server itself in a loop
async def looping_requests():
    while True:
        await asyncio.sleep(0.3)  # Wait for 300 milliseconds between each request
        response = requests.get("http://localhost:8000/")  # Send a request to the root endpoint
        logger.debug(f"Looping Request Response: {response.json()}")  # Log the response
