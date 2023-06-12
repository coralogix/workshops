from flask import Flask, make_response, request
from flask.logging import default_handler
from werkzeug.exceptions import HTTPException
import logging, json
import datetime, sys, ipaddr, random, os, binascii
import redis

# logging.basicConfig(format='%(levelname)s:%(message)s', level=logging.INFO)

# This simulator requres env variable REDIS_SERVICE_HOST
# Redis Setup
redis_host = os.getenv('REDIS_SERVICE_HOST')
redis_port = 6379
redis_password = ""

network = ipaddr.IPv4Network('255.255.255.255/0')

def redis_transact():  # simple redis example that will be picked up by auto-instrumentation
    try:
        transaction = ( (binascii.b2a_hex(os.urandom(8)).decode())) # generate random transaction number
        r = redis.StrictRedis(host=redis_host, port=redis_port, password=redis_password, decode_responses=True)
        r.set(transaction, "transaction success")
        r.expire(transaction, 3600)
        # msg = "transaction:" + r.get("transaction")
        return(transaction) # return transaction ID to be used for logging
    except Exception as e:
        print(e)

app = Flask(__name__)
app.logger.removeHandler(default_handler)
@app.route("/<path>")
def data(path):
    transaction=(redis_transact())
    random_ip = ipaddr.IPv4Address(random.randrange(int(network.network) + 1, int(network.broadcast) - 1)) # generate random IP address
    now = datetime.datetime.now()
    log_line_date_time = now.strftime("%Y-%m-%d %H:%M:%S")
    response = {
                "USER_IP": str(random_ip),
                "transaction": transaction,
                "result": path,
                "datetime": log_line_date_time
        }
    if path != "transact":
        response["transaction"]=failed
    response.content_type = "application/json"
    print(response.data)
    return response.data

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
    print(response.data)
    return response.data

# main 
if __name__ == '__main__':
    app.run(host='0.0.0.0')