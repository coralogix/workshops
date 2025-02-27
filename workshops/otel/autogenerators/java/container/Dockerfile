FROM eclipse-temurin:8-jdk-jammy

# Install necessary packages and download OpenTelemetry agents
RUN apt-get update && \
    apt-get -y install curl && \
    mkdir /home/code && \
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

# additional configs
# curl -L https://github.com/prometheus/jmx_exporter/releases/download/1.1.0/jmx_prometheus_javaagent-1.1.0.jar -o /opt/jmx_prometheus_javaagent-1.1.0.jar && \
# COPY ./jmx_exporter_config.yaml /home/code/
# Create JMX authentication files inside the container
# RUN echo "test readwrite" > /etc/jmxremote.access && \
#     echo "test test" > /etc/jmxremote.password && \
#     chmod 600 /etc/jmxremote.access /etc/jmxremote.password