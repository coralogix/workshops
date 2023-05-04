import requests
import os
# import logging, json_logging
# from datetime import datetime
from time import sleep
from random import random, seed
from sys import platform

if platform == "linux" or platform == "linux2":
    url = os.environ('PYTHON_TEST_URL') # Linux
elif platform == "darwin":
    url = os.getenv('PYTHON_TEST_URL') # Mac

seed(1)
x=1

def pythonrequests():
    response=requests.get(url)
    print(response.json())

while x:
    pythonrequests()
    y = random()
    # print('Sleeping: ', round(y,2))
    sleep(round(y,1))