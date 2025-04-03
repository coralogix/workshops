# agents/client_agent.py
import logging
import tiktoken


class ClientAgent:
    """
    Simulates a client AI agent that sends messages to a server AI.
    Tracks and logs token usage and estimated cost.
    """
    def __init__(self):
        self.name = "Client"
        self.logger = logging.getLogger("client")
        self.encoder = tiktoken.encoding_for_model("gpt-3.5-turbo")
        self.cumulative_tokens = 0

    def generate_message(self, last_server_response):
        """
        Generates a message based on the last server response.
        Logs the message with metadata.
        """
        if not last_server_response:
            msg = "Hi Server, how are you feeling today?"
        else:
            msg = f"I heard you said: '{last_server_response}'. What do you mean by that?"

        tokens = self.token_count(msg)
        self.cumulative_tokens += tokens

        self.logger.info(
            msg,
            extra={
                "agent": self.name,
                "tokens_used": tokens,
                "tokens_prompt": tokens,
                "tokens_completion": 0,
                "cumulative_tokens": self.cumulative_tokens,
                "cost_usd": round((tokens / 1000) * 0.0015, 6)
            }
        )
        return msg

    def token_count(self, text):
        """
        Calculates the number of tokens in a text.
        """
        return len(self.encoder.encode(text))

