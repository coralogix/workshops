import requests
import logging
from opentelemetry import trace

i = 0
logger = logging.getLogger()
logger.setLevel(logging.DEBUG)

tracer = trace.get_tracer_provider().get_tracer(__name__)
logger.info('Start processing...')

def httpget():
    with tracer.start_as_current_span("get reqbin"):
        r = requests.get('https://reqbin.com/echo/get/json', 
                headers={'Accept': 'application/json',
                'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.0.0 Safari/537.36'
                 })
    
        logger.debug('This is a debug message')
        logger.info('This is an info message')
        logger.warning('This is a warning message')
        logger.error('This is an error message')
        logger.critical('This is a critical message')
        logger.warning(f'Content: {r.json()}')

        return

for i in range(101):
    if i == 100:
        logger.info('Finished processing')
        break
    else:
        logger.debug('Calling httpget method...')
        httpget()
        continue
