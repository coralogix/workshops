# main.py
import logging
import time
from json_logger import JsonFormatter
from agents.client_agent import ClientAgent
from agents.server_agent import ServerAgent


def setup_logging():
    """
    Sets up the root logger to use JSON formatting.
    Clears existing handlers and applies JsonFormatter.
    """
    logger = logging.getLogger()
    logger.setLevel(logging.INFO)
    logger.handlers.clear()

    handler = logging.StreamHandler()
    formatter = JsonFormatter()
    handler.setFormatter(formatter)
    logger.addHandler(handler)


def run_conversation(rounds=5, delay=3):
    """
    Runs a simulated AI-to-AI conversation loop.
    Each round exchanges a message between Client and Server.
    """
    setup_logging()
    client = ClientAgent()
    server = ServerAgent()

    last_server_response = ""
    for i in range(rounds):
        logging.info(f"\n===== Round {i + 1} =====", extra={"agent": "system"})
        client_msg = client.generate_message(last_server_response)
        last_server_response = server.respond_to(client_msg)
        time.sleep(delay)


if __name__ == "__main__":
    run_conversation()
