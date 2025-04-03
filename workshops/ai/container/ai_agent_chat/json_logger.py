import logging
import json

class JsonFormatter(logging.Formatter):
    """
    Custom logging formatter for structured JSON logs.
    Adds fields like timestamp, agent name, token counts, and cost.
    """
    def format(self, record):
        log_record = {
            "timestamp": self.formatTime(record, self.datefmt),
            "level": record.levelname,
            "agent": getattr(record, "agent", "unknown"),
            "message": record.getMessage(),
            "tokens_used": getattr(record, "tokens_used", None),
            "tokens_prompt": getattr(record, "tokens_prompt", None),
            "tokens_completion": getattr(record, "tokens_completion", None),
            "cumulative_tokens": getattr(record, "cumulative_tokens", None),
            "cost_usd": getattr(record, "cost_usd", None)
        }
        # Remove None values for cleaner output
        return json.dumps({k: v for k, v in log_record.items() if v is not None})