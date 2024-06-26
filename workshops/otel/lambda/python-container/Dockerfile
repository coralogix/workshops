# Stage 1: Coralogix exporter
FROM coralogixrepo/coralogix-aws-lambda-telemetry-exporter:0.6.4 AS coralogix-extension

# Stage 2: AWS Lambda Python base image
FROM amazon/aws-lambda-python

# Copy Coralogix exporter from the first stage
WORKDIR /opt
COPY --from=coralogix-extension /opt/ .

# Make sure the extension is executable
RUN chmod +x /opt/extensions/coralogix*

# Set the working directory for Lambda function code
WORKDIR ${LAMBDA_TASK_ROOT}

# Copy requirements.txt and install dependencies
COPY requirements.txt ${LAMBDA_TASK_ROOT}
RUN pip install -r requirements.txt

RUN export PATH="$HOME/.local/bin:$PATH" && \
    opentelemetry-bootstrap -a install

    # Copy the function code
COPY lambda_function.py ${LAMBDA_TASK_ROOT}

# Copy the entrypoint script
COPY entrypoint.sh /entrypoint.sh

# Make the entrypoint script executable
RUN chmod +x /entrypoint.sh

# Set the entrypoint to the script
ENTRYPOINT ["/entrypoint.sh"]