import requests, os, sys
from sys import platform
from time import sleep
from random import random, seed
import json_logging, json

# This simulator requres env variables PYTHON_TEST_URL which is the IP address of the server simulator port 5001
# and PYTHON_TEST_URLBAD which is 0 or 1

json_logging.init_non_web(enable_json=True)
logger = logging.getLogger("python-reqs")
logger.setLevel(logging.DEBUG)
logger.addHandler(logging.StreamHandler(sys.stdout))

# get the env variable PYTHON_TEST_URLBAD 0 or 1 to be used for simulating a bad deployment
if platform == "linux" or platform == "linux2":
    url = os.environ('PYTHON_TEST_URL') # Mac
    isurlbad = int(os.environ('PYTHON_TEST_URLBAD'))
elif platform == "darwin":
    url = os.getenv('PYTHON_TEST_URL') # Linux
    isurlbad = int(os.getenv('PYTHON_TEST_URLBAD')) # Mac

seed(1)
x=1

# the bad url- adding /bad to the req
urlbad = url + "/bad" 

# if the ISURLBAD simulation is 1 then direct reqs at urlbad which adds /bad to the req to simulate a bad deployment
# however only do this less than 5% of time so simulate an intermittent problem

def pythonreqs():
    try: 
        if random() <= .05 and isurlbad==1 :
            response=requests.get(urlbad)
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