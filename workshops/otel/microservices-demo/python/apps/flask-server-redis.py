from flask import Flask, make_response, request
from flask.logging import default_handler
from werkzeug.exceptions import HTTPException
from time import sleep
import logging, json, datetime, sys, ipaddr, os, binascii, redis
from random import seed
import random
from opentelemetry.instrumentation.logging import LoggingInstrumentor

print('Coralogix Transaction Server Demo')
# logging.basicConfig(format='%(levelname)s:%(message)s', level=logging.INFO)

# This simulator requres env variable REDIS_SERVICE_HOST or set to FALSE for no redis transaction
# Redis Setup
redis_host = os.getenv('REDIS_SERVICE_HOST')
redis_port = 6379
redis_password = ""

slow_server = os.environ.get('SLOW_SERVER')

LoggingInstrumentor(set_logging_format=True)
LoggingInstrumentor(log_level=logging.DEBUG)
logging.basicConfig(format='%(levelname)s:%(message)s SPANID=%(otelSpanID)s TRACEID=%(otelTraceID)s SERVICENAME=%(otelServiceName)s', level=logging.DEBUG)

seed()

network = ipaddr.IPv4Network('255.255.255.255/0')

def redis_transact():  # simple redis example that will be picked up by auto-instrumentation
    try:
        transaction = ( (binascii.b2a_hex(os.urandom(8)).decode())) # generate random transaction number
        r = redis.StrictRedis(host=redis_host, port=redis_port, password=redis_password, decode_responses=True)
        r.set(transaction, "transaction success")
        r.expire(transaction, 3600)
        return(transaction) # return transaction ID to be used for logging
    except Exception as e:
        print(e)

app = Flask(__name__)
# app.logger.removeHandler(default_handler)
@app.route("/<path>")
def data(path):
    if slow_server=="YES":
        y = random.uniform(.75, 1)
        sleep(y)
    if redis_host != "FALSE":
        transaction=(redis_transact())
    random_ip = ipaddr.IPv4Address(random.randrange(int(network.network) + 1, int(network.broadcast) - 1)) # generate random IP address
#   random_ip = ipaddr.IPv4Address(random.randrange(int(network.network) + 1, int(network.broadcast))) # generate random IP address
    now = datetime.datetime.now()
    log_line_date_time = now.strftime("%Y-%m-%d %H:%M:%S")
    if path == "transact":
        response = json.dumps({
            "USER_IP": str(random_ip),
            "transaction": transaction,
            "result": path,
            "datetime": log_line_date_time
        })
    # print(response)
    logging.info(response)
    return response

@app.errorhandler(HTTPException)
def handle_exception(e):
    response = e.get_response()
    random_ip = ipaddr.IPv4Address(random.randrange(int(network.network) + 1, int(network.broadcast) - 1)) # generate random IP address
    now = datetime.datetime.now()
    log_line_date_time = now.strftime("%Y-%m-%d %H:%M:%S")
    response.data = json.dumps({
    "code": e.code,
    "name": e.name,
    "description": e.description,
    "USER_IP": str(random_ip),
    "transaction": "failed",
    "result": "failed",
    "datetime": log_line_date_time
    })
    response.content_type = "application/json"
    logging.info(response.data)
    return response.data

if __name__ == '__main__':
    app.run(host='0.0.0.0')