import datetime, sys, json_logging, logging, uvicorn, ipaddr, random
import redis
import os, binascii
from fastapi import FastAPI, HTTPException, Request
from fastapi.responses import JSONResponse

# This simulator requres env variable REDIS_SERVICE_HOST
# Redis Setup
redis_host = os.getenv('REDIS_SERVICE_HOST')
redis_port = 6379
redis_password = ""

# json_logging.init_fastapi(enable_json=True)
app = FastAPI()
json_logging.init_fastapi(enable_json=True)
# json_logging.init_request_instrument(app)

logger = logging.getLogger("transaction-logger")
logger.setLevel(logging.INFO)
logger.addHandler(logging.StreamHandler(sys.stdout))

network = ipaddr.IPv4Network('255.255.255.255/0')

def redis_transact():  # simple redis example that will be picked up by auto-instrumentation
    try:
        transaction = ( (binascii.b2a_hex(os.urandom(8)).decode())) # generate random transaction number
        r = redis.StrictRedis(host=redis_host, port=redis_port, password=redis_password, decode_responses=True)
        r.set("transaction", transaction)
        msg = "transaction:" + r.get("transaction")
        return(transaction) # return transaction ID to be used for logging
    except Exception as e:
        print(e)

@app.get('/{path}')
async def read_path(path:str):
    random_ip = ipaddr.IPv4Address(random.randrange(int(network.network) + 1, int(network.broadcast) - 1)) # generate random IP address
    transaction=(redis_transact())
    detail = {
                "USER_IP": str(random_ip),
                "transaction": transaction,
                "result": path
        }
    logger.info("transactionlog", extra={'props': detail})
    if path == "transact":
        return detail
    else:
        raise HTTPException(
            status_code=404, detail=
            detail
        )

if __name__ == "__main__":
    uvicorn.run(app, host='0.0.0.0', port=5001)