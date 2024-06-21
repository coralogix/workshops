FROM eclipse-temurin:8-jdk-jammy
RUN apt-get update && \
    apt-get -y install curl && \
    mkdir /home/code
COPY target/java-app-1.0-SNAPSHOT.jar /home/code
COPY run-client.sh /home/code
COPY opentelemetry-javaagent.jar /home/code