import datetime, logging, sys, json_logging, fastapi, uvicorn
import redis
import os, binascii

# Redis Setup
redis_host = os.getenv('REDIS_SERVICE_HOST')
redis_port = 6379
redis_password = ""

app = fastapi.FastAPI()
json_logging.init_fastapi(enable_json=True)
json_logging.init_request_instrument(app)

logger = logging.getLogger("transaction-logger")
logger.setLevel(logging.DEBUG)
logger.addHandler(logging.StreamHandler(sys.stdout))

def redis_transact():  # simple redis example that will be picked up by auto-instrumentation
    try:
        transaction = ( (binascii.b2a_hex(os.urandom(8)).decode()))
        r = redis.StrictRedis(host=redis_host, port=redis_port, password=redis_password, decode_responses=True)
        r.set("msg:transaction", transaction)
        msg = "transaction:" + r.get("msg:transaction")
        return(transaction)
        # print(msg)
    except Exception as e:
        print(e)

@app.get('/')
def home():
    transaction=(redis_transact())
    logger.info("transaction log", extra={'props': {"transaction": transaction}})
if __name__ == "__main__":
    uvicorn.run(app, host='0.0.0.0', port=5001)