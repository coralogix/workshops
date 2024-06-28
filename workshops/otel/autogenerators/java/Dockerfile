FROM eclipse-temurin:8-jdk-jammy

RUN apt-get update && \
    apt-get -y install curl && \
    mkdir /home/code && \
    curl -L https://github.com/open-telemetry/opentelemetry-java-instrumentation/releases/latest/download/opentelemetry-javaagent.jar -o /opt/opentelemetry-javaagent.jar && \
    chmod 755 /opt/opentelemetry-javaagent.jar

WORKDIR /home/code

COPY ./target/java-app-1.0-SNAPSHOT.jar /home/code/
COPY ./entrypoint.sh /home/code/
COPY ./src/main/resources/log4j2.xml /home/code/
RUN chmod +x /home/code/entrypoint.sh

ENTRYPOINT ["/bin/sh", "-c", "/home/code/entrypoint.sh"]
