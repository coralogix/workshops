FROM eclipse-temurin:8-jdk-jammy

# Install necessary packages and download OpenTelemetry agents
RUN apt-get update && \
    apt-get -y install curl && \
    mkdir /home/code && \
    curl -L https://github.com/open-telemetry/opentelemetry-java-instrumentation/releases/latest/download/opentelemetry-javaagent.jar -o /opt/opentelemetry-javaagent.jar && \
    curl -L https://github.com/open-telemetry/opentelemetry-java-contrib/releases/download/v1.44.0/opentelemetry-jmx-metrics.jar -o /opt/opentelemetry-java-contrib-jmx-metrics.jar && \
    chmod 755 /opt/opentelemetry-javaagent.jar && \
    chmod 755 /opt/opentelemetry-java-contrib-jmx-metrics.jar

WORKDIR /home/code

# Copy application files
COPY ./target/java-app-1.0-SNAPSHOT.jar /home/code/
COPY ./entrypoint.sh /home/code/
COPY ./src/main/resources/log4j2.xml /home/code/

# Create JMX authentication files inside the container
RUN echo "test readwrite" > /etc/jmxremote.access && \
    echo "test test" > /etc/jmxremote.password && \
    chmod 600 /etc/jmxremote.access /etc/jmxremote.password

# Ensure entrypoint script is executable
RUN chmod +x /home/code/entrypoint.sh

ENTRYPOINT ["/bin/sh", "/home/code/entrypoint.sh"]
