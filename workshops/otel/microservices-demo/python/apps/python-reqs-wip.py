import os
import sys
import json
import logging
import requests
from time import sleep
from random import random, seed
from opentelemetry.instrumentation.logging import LoggingInstrumentor

seed()
x = 1

# Set up logging
LoggingInstrumentor(set_logging_format=True)
LoggingInstrumentor(log_level=logging.DEBUG)
logging.basicConfig(
    format='%(levelname)s:%(message)s SPANID=%(otelSpanID)s TRACEID=%(otelTraceID)s SERVICENAME=%(otelServiceName)s',
    level=logging.DEBUG
)

env_url = os.environ.get('PYTHON_TEST_URL')
env_url_good = os.environ.get('PYTHON_TEST_URLGOOD')
one_shot = os.environ.get('PYTHON_ONESHOT')

def python_reqs():
    try:
        if one_shot == "NO":
            bad_chance = random()
            url = env_url if bad_chance <= 0.85 and env_url_good == "BAD" else env_url + "/transact"
        else:
            bad_chance = 1
            url = env_url

        response = requests.get(url)
        logging.info(response.json())
        if one_shot == "YES":
            sys.exit("Oneshot")

    except requests.exceptions.RequestException as err:
        log_dict = {'error': str(err)}
        print(json.dumps(log_dict, indent=2, separators=(',', ':')))

while x:
    python_reqs()
    sleep(round(random(), 1))