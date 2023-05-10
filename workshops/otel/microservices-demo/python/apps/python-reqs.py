import requests, os, sys
from sys import platform
from time import sleep
from random import random, seed
import logging, json_logging, json

json_logging.init_non_web(enable_json=True)
logger = logging.getLogger("python-reqs")
logger.setLevel(logging.DEBUG)
logger.addHandler(logging.StreamHandler(sys.stdout))

if platform == "linux" or platform == "linux2":
    url = os.environ('PYTHON_TEST_URL') # Mac
    isurlbad = int(os.environ('PYTHON_TEST_URLBAD'))
elif platform == "darwin":
    url = os.getenv('PYTHON_TEST_URL') # Linux
    isurlbad = int(os.getenv('PYTHON_TEST_URLBAD')) # Mac

seed(1)
x=1

urlbad = url + "/bad" 

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