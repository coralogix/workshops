FROM node:lts-slim

# Create working directory
RUN mkdir /home/code

# Copy application and entrypoint script
COPY app.js /home/code/
COPY entrypoint.sh /home/code/

# Set working directory
WORKDIR /home/code

# Install necessary packages and dependencies
RUN apt -y update && \
    apt install -y curl jq && \
    npm install --save uuid && \
    npm install --save pino && \
    apt -y autoremove && apt-get -y autoclean

# Ensure entrypoint.sh is executable
RUN chmod +x /home/code/entrypoint.sh

# Set entrypoint
ENTRYPOINT ["/bin/sh", "/home/code/entrypoint.sh"]
