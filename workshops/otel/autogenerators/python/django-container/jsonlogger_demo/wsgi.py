import os
from django.core.wsgi import get_wsgi_application

# Import and initialize OpenTelemetry logging context BEFORE the app
from opentelemetry.instrumentation.logging import LoggingInstrumentor

# Inject trace_id and span_id into logs
LoggingInstrumentor().instrument(set_logging_format=True)

# Set the Django settings module
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'jsonlogger_demo.settings')

# Create the WSGI application
application = get_wsgi_application()
