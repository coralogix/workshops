java \
-javaagent:/opt/opentelemetry-javaagent.jar \
-Dlog4j.configurationFile=classpath:log4j2.xml \
-Dcom.sun.management.jmxremote \
-Dcom.sun.management.jmxremote.port=9999 \
-Dcom.sun.management.jmxremote.authenticate=false \
-Dcom.sun.management.jmxremote.ssl=false \
-jar java-app-1.0-SNAPSHOT.jar
# -javaagent:/opt/jmx_prometheus_javaagent-1.1.0.jar=9090:jmx_exporter_config.yaml \