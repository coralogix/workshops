import os
import sys

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

SECRET_KEY = 'fake-key'
DEBUG = False
ROOT_URLCONF = 'jsonlogger_demo.urls'
ALLOWED_HOSTS = ['*','example.com', 'yourdomain.com', '127.0.0.1']
INSTALLED_APPS = ['django.contrib.contenttypes', 'app']

MIDDLEWARE = [
    'django.middleware.common.CommonMiddleware',
    'jsonlogger_demo.middleware.JsonRequestLoggingMiddleware',  # custom logger
]

# Logging config
LOGGING = {
    'version': 1,
    'disable_existing_loggers': False,
    'formatters': {
        'json': {
            '()': 'pythonjsonlogger.jsonlogger.JsonFormatter',
            'format': '%(asctime)s %(levelname)s %(message)s %(request_method)s %(path)s',
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

WSGI_APPLICATION = 'jsonlogger_demo.wsgi.application'
