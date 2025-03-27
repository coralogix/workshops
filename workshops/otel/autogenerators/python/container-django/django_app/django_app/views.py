import logging
from django.http import HttpResponse

# Set up a logger for this module
logger = logging.getLogger(__name__)

def home(request):
    # Extract the request ID from headers (if present)
    request_id = request.headers.get("X-Request-ID", "unknown")

    # Log the incoming request
    logger.info("Django received a request", extra={"request_id": request_id})

    # Respond to the client
    return HttpResponse(f"Hello from Django! request_id: {request_id}")
