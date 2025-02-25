java -javaagent:/opt/opentelemetry-javaagent.jar \
-Dlog4j.configurationFile=classpath:log4j2.xml \
-Dcom.sun.management.jmxremote \
-Dcom.sun.management.jmxremote.port=9999 \
-Dcom.sun.management.jmxremote.authenticate=true \
-Dcom.sun.management.jmxremote.ssl=false \
-Dcom.sun.management.jmxremote.access.file=/etc/jmxremote.access \
-Dcom.sun.management.jmxremote.password.file=/etc/jmxremote.password \
-jar java-app-1.0-SNAPSHOT.jar