import requests, os, sys, json, logging
from time import sleep
from random import random, seed
from opentelemetry.instrumentation.logging import LoggingInstrumentor

# This simulator requres env variables PYTHON_TEST_URL which is the IP address of the server simulator port 5001
# and PYTHON_TEST_URLBAD which is GOOD or BAD

isurlgood = os.environ.get('PYTHON_TEST_URLGOOD')
envurl = os.environ.get('PYTHON_TEST_URL')
oneshot = os.env.get('PYTHON_ONESHOT')

seed()
x=1

LoggingInstrumentor(set_logging_format=True)
LoggingInstrumentor(log_level=logging.DEBUG)
logging.basicConfig(format='%(levelname)s:%(message)s SPANID=%(otelSpanID)s TRACEID=%(otelTraceID)s SERVICENAME=%(otelServiceName)s', level=logging.DEBUG)

# if the ISURLGOOD simulation is 0 then direct reqs at urlbad which adds /bad to the req to simulate a bad deployment
# however only do this less than 5% of time so simulate an intermittent problem

def pythonreqs():
    try: 
        if oneshot==("NO"): # if its not one shot run standard chance
            badchance = random()
            if badchance <= .85 and isurlgood=="BAD":
                url = envurl # req a bad url
            else:
                url = envurl + "/transact" # req a good url
        else: # if this is a oneshot bad trace req a bad url
            badchance = 1
            url = envurl
            sys.exit("Oneshot") # exit since its a one shot op
        response = requests.get(url)
        logging.info(response.json())
    except requests.exceptions.RequestException as err:
        log_dict = {'error': str(err),   
            }
        print(json.dumps(log_dict,indent=2,separators=(',', ':')))

while x:
    pythonreqs()
    y = random()
    sleep(round(y,1))