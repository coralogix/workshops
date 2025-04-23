from llm_tracekit import OpenAIInstrumentor, setup_export_to_coralogix
from openai import OpenAI

# Optional: Configure sending spans to Coralogix
# Reads Coralogix connection details from the following environment variables:
# - CX_TOKEN
# - CX_ENDPOINT
setup_export_to_coralogix(
    service_name="ai-service",
    application_name="ai-application",
    subsystem_name="ai-subsystem",
    capture_content=True,
)

# Activate instrumentation
OpenAIInstrumentor().instrument()

# Example OpenAI Usage
client = OpenAI()
response = client.chat.completions.create(
    model="gpt-4o-mini",
    messages=[
        {"role": "user", "content": "Write a short poem on open telemetry."},
    ],
)

# Print the response content
print(response.choices[0].message.content)