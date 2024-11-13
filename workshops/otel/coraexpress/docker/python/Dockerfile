# Start from the official Python image
FROM python:3.13-slim-bookworm

# Set the working directory
WORKDIR /app

# Copy the Python script
COPY database_test.py /app/database_test.py

# Install system dependencies and Python packages
RUN apt -y update && \
    apt -y install gcc libpq-dev && \
    pip install --upgrade pip && \
    pip install requests psycopg2-binary pymongo redis && \
    apt -y autoremove && apt-get -y autoclean && rm -rf /var/lib/apt/lists/*

# Run the script
CMD ["python", "database_test.py"]