export OTEL_SERVICE_NAME=cx-java-autogen
export OTEL_RESOURCE_ATTRIBUTES="cx.application.name=cx-java-autogen,cx.subsystem.name=cx-java-autogen"

java -javaagent:/opt/opentelemetry-javaagent.jar \
-Dlog4j.configurationFile=classpath:log4j2.xml \
-jar target/java-app-1.0-SNAPSHOT.jar