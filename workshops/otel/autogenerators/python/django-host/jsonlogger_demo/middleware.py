import logging

logger = logging.getLogger("request_logger")

class JsonRequestLoggingMiddleware:
    def __init__(self, get_response):
        self.get_response = get_response

    def __call__(self, request):
        response = self.get_response(request)

        logger.info(
            "Request received",
            extra={
                "request_method": request.method,
                "path": request.get_full_path()
            }
        )

        return response
