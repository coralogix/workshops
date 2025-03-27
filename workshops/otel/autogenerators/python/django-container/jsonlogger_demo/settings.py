import os
import sys

# Base directory
BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

# Security and debug
SECRET_KEY = 'fake-key'
DEBUG = True
ALLOWED_HOSTS = ['*', 'example.com', 'yourdomain.com', '127.0.0.1']

# Installed apps
INSTALLED_APPS = [
    'django.contrib.contenttypes',
    'app',
]

# Middleware
MIDDLEWARE = [
    'django.middleware.common.CommonMiddleware',
    'jsonlogger_demo.middleware.JsonRequestLoggingMiddleware',  # custom logger
]

# URL and WSGI
ROOT_URLCONF = 'jsonlogger_demo.urls'
WSGI_APPLICATION = 'jsonlogger_demo.wsgi.application'

# Console output
print("DEBUG:", DEBUG, "ALLOWED_HOSTS:", ALLOWED_HOSTS, file=sys.stderr)

# Logging with OpenTelemetry trace context
LOGGING = {
    'version': 1,
    'disable_existing_loggers': False,
    'formatters': {
        'json': {
            '()': 'pythonjsonlogger.jsonlogger.JsonFormatter',
            'format': '%(asctime)s %(levelname)s %(message)s %(request_method)s %(path)s %(otelTraceID)s %(otelSpanID)s',
        },
    },
    'handlers': {
        'console': {
            'class': 'logging.StreamHandler',
            'stream': sys.stdout,
            'formatter': 'json',
        },
    },
    'loggers': {
        'django.request': {
            'handlers': ['console'],
            'level': 'INFO',
            'propagate': False,
        },
        'request_logger': {
            'handlers': ['console'],
            'level': 'INFO',
            'propagate': False,
        },
    },
}
