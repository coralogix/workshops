import requests
import os
from time import sleep
from random import random, seed
from sys import platform
import logging
import json_logging
import sys

json_logging.init_non_web(enable_json=True)
logger = logging.getLogger("python-reqs")
logger.setLevel(logging.DEBUG)
logger.addHandler(logging.StreamHandler(sys.stdout))

# logger.info("test logging statement")

# logging.getLogger("requests").setLevel(logging.INFO)
# from http.client import HTTPConnection
# HTTPConnection.debuglevel = 1


if platform == "linux" or platform == "linux2":
    url = os.environ('PYTHON_TEST_URL') # Linux
elif platform == "darwin":
    url = os.getenv('PYTHON_TEST_URL') # Mac

seed(1)
x=1

def pythonrequests():
    response=requests.get(url)
    logger.info(response)

while x:
    pythonrequests()
    y = random()
    # print('Sleeping: ', round(y,2))
    sleep(round(y,1))