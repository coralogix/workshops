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

if platform == "linux" or platform == "linux2":
    url = os.environ('PYTHON_TEST_URL') # Linux
elif platform == "darwin":
    url = os.getenv('PYTHON_TEST_URL') # Mac

    urlbad = url + "/bad"

seed(1)
x=1

while x:
    if random() <= .05 :
        response=requests.get(urlbad)
    else :
        response=requests.get(url)
    logger.info(response)
    y = random()
    sleep(round(y,1))