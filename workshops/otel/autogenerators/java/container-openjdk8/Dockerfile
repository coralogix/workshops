FROM openjdk:8u232-jdk-slim

# Install necessary packages and download OpenTelemetry agents
RUN apt-get update && \
    apt-get install -y curl && \
    mkdir -p /home/code && \
    curl -L https://github.com/open-telemetry/opentelemetry-java-instrumentation/releases/latest/download/opentelemetry-javaagent.jar -o /opt/opentelemetry-javaagent.jar && \
    chmod 755 /opt/*.jar

WORKDIR /home/code

# Copy application files
COPY ./target/java-app-1.0-SNAPSHOT.jar /home/code/
COPY ./entrypoint.sh /home/code/
COPY ./src/main/resources/log4j2.xml /home/code/

# Ensure entrypoint script is executable
RUN chmod +x /home/code/entrypoint.sh

ENTRYPOINT ["/bin/sh", "/home/code/entrypoint.sh"]
