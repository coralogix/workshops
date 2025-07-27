FROM eclipse-temurin:8-jdk-jammy

# Install necessary packages and download OpenTelemetry agents
RUN apt-get update && \
    apt-get -y install curl && \
    mkdir /home/code && \
    curl -L https://github.com/open-telemetry/opentelemetry-java-instrumentation/releases/latest/download/opentelemetry-javaagent.jar -o /opt/opentelemetry-javaagent.jar && \
    chmod 755 /opt/*.jar

# Copy application files
COPY ./server/target/java-app-1.0-SNAPSHOT.jar /home/code/server/server.jar
COPY ./server/entrypoint.sh /home/code/server/entrypoint.sh
COPY ./server/src/main/resources/log4j2.xml /home/code/server/log4j2.xml

COPY ./client/target/java-app-1.0-SNAPSHOT.jar /home/code/client/client.jar
COPY ./client/entrypoint.sh /home/code/client/entrypoint.sh
COPY ./client/src/main/resources/log4j2.xml /home/code/client/log4j2.xml


# Ensure entrypoint script is executable        
RUN chmod +x /home/code/server/entrypoint.sh && \
    chmod +x /home/code/client/entrypoint.sh