import datetime, sys, json_logging, fastapi, uvicorn, ipaddr, random
import redis
import os, binascii

# This simulator requres env variable REDIS_SERVICE_HOST
 
# Redis Setup
redis_host = os.getenv('REDIS_SERVICE_HOST')
redis_port = 6379
redis_password = ""

app = fastapi.FastAPI()
json_logging.init_fastapi(enable_json=True)
json_logging.init_request_instrument(app)

logger = logging.getLogger("transaction-logger")
logger.setLevel(logging.INFO)
logger.addHandler(logging.StreamHandler(sys.stdout))

network = ipaddr.IPv4Network('255.255.255.255/0')

def redis_transact():  # simple redis example that will be picked up by auto-instrumentation
    try:
        transaction = ( (binascii.b2a_hex(os.urandom(8)).decode())) # generate random transaction number
        r = redis.StrictRedis(host=redis_host, port=redis_port, password=redis_password, decode_responses=True)
        r.set("msg:transaction", transaction)
        msg = "transaction:" + r.get("msg:transaction")
        return(transaction) # return transaction ID to be used for logging
    except Exception as e:
        print(e)

@app.get('/')
def home():
    random_ip = ipaddr.IPv4Address(random.randrange(int(network.network) + 1, int(network.broadcast) - 1)) # generate random IP address

    transaction=(redis_transact())
    logger.info("transactionlog", extra={'props': {'user_IP': str(random_ip),'transaction': transaction}})
if __name__ == "__main__":
    uvicorn.run(app, host='0.0.0.0', port=5001)