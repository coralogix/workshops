unset OTEL_EXPORTER_OTLP_ENDPOINT
unset OTEL_METRICS_EXPORTER
unset OTEL_TRACES_EXPORTER
unset OTEL_NODE_RESOURCE_DETECTORS
unset OTEL_EXPORTER_OTLP_ENDPOINT
unset OTEL_SERVICE_NAME
export OTEL_SERVICE_NAME=cx-java-autogen
export OTEL_RESOURCE_ATTRIBUTES="cx.application.name=cx-java-autogen,cx.subsystem.name=cx-java-autogen"

java -javaagent:/opt/opentelemetry-javaagent.jar \
-Dlog4j.configurationFile=classpath:log4j2.xml \
-Dcom.sun.management.jmxremote \
-Dcom.sun.management.jmxremote.port=9999 \
-Dcom.sun.management.jmxremote.authenticate=false \
-Dcom.sun.management.jmxremote.ssl=false \
-jar target/java-app-1.0-SNAPSHOT.jar