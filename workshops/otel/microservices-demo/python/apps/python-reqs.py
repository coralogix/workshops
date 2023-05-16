import requests, os, sys
from time import sleep
from random import random, seed
import json_logging, json, logging

# This simulator requres env variables PYTHON_TEST_URL which is the IP address of the server simulator port 5001
# and PYTHON_TEST_URLBAD which is 0 or 1

json_logging.init_non_web(enable_json=True)
logger = logging.getLogger("python-reqs")
logger.setLevel(logging.DEBUG)
logger.addHandler(logging.StreamHandler(sys.stdout))

isurlgood = os.environ.get('PYTHON_TEST_URLGOOD')
envurl = os.environ.get('PYTHON_TEST_URL')

seed(1)
x=1


# if the ISURLGOOD simulation is 0 then direct reqs at urlbad which adds /bad to the req to simulate a bad deployment
# however only do this less than 5% of time so simulate an intermittent problem

def pythonreqs():
    try: 
        badchance = random()
        if badchance <= .25 and isurlgood=="BAD":
            url = envurl + "/bad" 
        else:
            url = envurl + "/transact"
        
        response = requests.get(url)
        jsonResponse=response.json()

        if badchance <= .25 and isurlgood=="BAD":
            logger.info("transactionlog", extra={'props': {'user_IP': (jsonResponse["detail"]["USER_IP"]),'transaction': (jsonResponse["detail"]["transaction"]),'result': (jsonResponse["detail"]["result"])}})
        else:
            logger.info("transactionlog", extra={'props': {'user_IP': (jsonResponse["USER_IP"]),'transaction': (jsonResponse["transaction"]), 'result': (jsonResponse["result"])}})

    except requests.exceptions.RequestException as err:
        log_dict = {'error': str(err),   
            }
        print(json.dumps(log_dict,indent=2,separators=(',', ':')))

#loop requests with a 5% chance of generating a 404 by requesting /bad from the fastapi server

while x:
    pythonreqs()
    y = random()
    sleep(round(y,1))