import sys
import json
import logging
import logging.config

# --- Basic Django Settings ---
SECRET_KEY = 'dev-key'
DEBUG = True
ALLOWED_HOSTS = ['*']
ROOT_URLCONF = 'django_app.urls'
INSTALLED_APPS = []
MIDDLEWARE = []

# --- Custom JSON Formatter ---
class JsonFormatter(logging.Formatter):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)

    def format(self, record):
        log_record = {
            "time": self.formatTime(record, self.datefmt),
            "level": record.levelname,
            "name": record.name,
            "message": record.getMessage(),
            "trace_id": getattr(record, "otelTraceID", "N/A"),
            "span_id": getattr(record, "otelSpanID", "N/A"),
        }
        if record.exc_info:
            log_record["exception"] = self.formatException(record.exc_info)
        return json.dumps(log_record)

# --- Django Logging Config ---
LOGGING = {
    'version': 1,
    'disable_existing_loggers': False,
    'formatters': {
        'json': {
            '()': JsonFormatter,
        },
    },
    'handlers': {
        'console': {
            'class': 'logging.StreamHandler',
            'stream': sys.stdout,
            'formatter': 'json',
        },
    },
    'root': {
        'handlers': ['console'],
        'level': 'DEBUG',
    },
}

# --- Force Logging Config to Apply Immediately ---
logging.config.dictConfig(LOGGING)

print("settings.py loaded — DEBUG:", DEBUG, "ALLOWED_HOSTS:", ALLOWED_HOSTS)
