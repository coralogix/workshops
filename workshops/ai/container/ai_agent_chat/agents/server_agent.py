# agents/server_agent.py
import logging
import os
import tiktoken
from openai import OpenAI
from dotenv import load_dotenv


class ServerAgent:
    """
    Simulates a server AI agent that responds to messages from a client AI.
    Uses OpenAI API to generate replies and logs token and cost data.
    """
    def __init__(self):
        load_dotenv()
        self.client = OpenAI()
        self.name = "Server"
        self.logger = logging.getLogger("server")
        self.encoder = tiktoken.encoding_for_model("gpt-3.5-turbo")
        self.cumulative_tokens = 0

    def respond_to(self, client_message):
        """
        Responds to a client message using OpenAI's Chat API.
        Logs response metadata including token usage and cost.
        """
        self.logger.info(
            f"Received message: {client_message}",
            extra={"agent": self.name}
        )

        try:
            response = self.client.chat.completions.create(
                model="gpt-3.5-turbo",
                messages=[
                    {"role": "system", "content": "You are a helpful AI server."},
                    {"role": "user", "content": client_message}
                ],
                temperature=0.7,
                max_tokens=100
            )

            reply = response.choices[0].message.content.strip()
            usage = response.usage
            prompt_tokens = usage.prompt_tokens
            completion_tokens = usage.completion_tokens
            total_tokens = usage.total_tokens
            cost = self.estimate_cost(prompt_tokens, completion_tokens)
            self.cumulative_tokens += total_tokens

        except Exception as e:
            reply = f"Error generating response: {e}"
            prompt_tokens = self.token_count(client_message)
            completion_tokens = 0
            total_tokens = prompt_tokens
            cost = 0.0

        self.logger.info(
            reply,
            extra={
                "agent": self.name,
                "tokens_used": total_tokens,
                "tokens_prompt": prompt_tokens,
                "tokens_completion": completion_tokens,
                "cumulative_tokens": self.cumulative_tokens,
                "cost_usd": cost
            }
        )
        return reply

    def estimate_cost(self, prompt_tokens, completion_tokens):
        """
        Estimate USD cost for the API call based on OpenAI's pricing.
        """
        return round((prompt_tokens / 1000) * 0.0015 + (completion_tokens / 1000) * 0.002, 6)

    def token_count(self, text):
        """
        Counts tokens in a text manually (used as a fallback).
        """
        return len(self.encoder.encode(text))