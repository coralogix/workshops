FROM eclipse-temurin:8-jdk-jammy

RUN apt-get update && \
    apt-get -y install curl && \
    mkdir /home/code

WORKDIR /home/code

COPY ./target/java-app-1.0-SNAPSHOT.jar /home/code/
COPY ./entrypoint.sh /home/code/
COPY ./src/main/resources/log4j2.xml /home/code/
RUN chmod +x /home/code/entrypoint.sh

ENTRYPOINT ["/bin/sh", "/home/code/entrypoint.sh"]
