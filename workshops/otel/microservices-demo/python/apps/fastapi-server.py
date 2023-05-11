import datetime, logging, sys, json_logging, fastapi, uvicorn, ipaddr, random

app = fastapi.FastAPI()
json_logging.init_fastapi(enable_json=True)
json_logging.init_request_instrument(app)

# init the logger as usual
logger = logging.getLogger("server-logger")
logger.setLevel(logging.INFO)
logger.addHandler(logging.StreamHandler(sys.stdout))

network = ipaddr.IPv4Network('255.255.255.255/0')

@app.get('/')
def home():
    random_ip = ipaddr.IPv4Address(random.randrange(int(network.network) + 1, int(network.broadcast) - 1)) # generate random IP address
    logger.info("Extra Log", extra={'props': {'user_IP': str(random_ip)}})

if __name__ == "__main__":
    uvicorn.run(app, host='0.0.0.0', port=5001)