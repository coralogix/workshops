FROM python:slim

# Create working directory
RUN mkdir -p /home/code

# Copy application code
COPY app.py /home/code/

# Set working directory
WORKDIR /home/code

# Install required packages
RUN apt -y update && \
    pip install --upgrade pip && \
    pip install \
        requests \
        "Flask <3" \
        "Werkzeug <3" \
        jaeger-client \
        opentracing && \
    apt -y autoremove && apt-get -y autoclean

# Start app directly
CMD ["python", "app.py"]
