import requests, os, sys
from time import sleep
from random import random, seed
import json, logging

# This simulator requres env variables PYTHON_TEST_URL which is the IP address of the server simulator port 5001
# and PYTHON_TEST_URLBAD which is GOOD or BAD

isurlgood = os.environ.get('PYTHON_TEST_URLGOOD')
envurl = os.environ.get('PYTHON_TEST_URL')

seed()
x=1

logging.basicConfig(level=logging.INFO)

# if the ISURLGOOD simulation is 0 then direct reqs at urlbad which adds /bad to the req to simulate a bad deployment
# however only do this less than 5% of time so simulate an intermittent problem

def pythonreqs():
    try: 
        badchance = random()
        if badchance <= .95 and isurlgood=="BAD":
            url = envurl
        else:
            url = envurl + "/transact"
        response = requests.get(url)
        logging.info(response.json())
    except requests.exceptions.RequestException as err:
        log_dict = {'error': str(err),   
            }
        print(json.dumps(log_dict,indent=2,separators=(',', ':')))

#loop requests with a 5% chance of generating a 404 by requesting /bad from the fastapi server

while x:
    pythonreqs()
    y = random()
    sleep(round(y,1))