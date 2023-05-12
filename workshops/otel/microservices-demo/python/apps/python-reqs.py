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

url = os.environ.get('PYTHON_TEST_URL')
isurlgood = os.environ.get('PYTHON_TEST_URLGOOD')

seed(1)
x=1

# the bad url- adding /bad to the req
badurl = url + "/bad" 

# if the ISURLGOOD simulation is 0 then direct reqs at urlbad which adds /bad to the req to simulate a bad deployment
# however only do this less than 5% of time so simulate an intermittent problem

def pythonreqs():
    try: 
        if random() <= .2 and isurlgood=="BAD" :
            response=requests.get(badurl)
        else :
            response=requests.get(url)
        logger.info(response)   
    except requests.exceptions.RequestException as err:
        log_dict = {'error': str(err),   
            }
        print(json.dumps(log_dict,indent=2,separators=(',', ':')))

#loop requests with a 5% chance of generating a 404 by requesting /bad from the fastapi server

while x:
    pythonreqs()
    y = random()
    sleep(round(y,1))