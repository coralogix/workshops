java \
-javaagent:/opt/opentelemetry-javaagent.jar \
-Dlog4j.configurationFile=classpath:log4j2.xml \
-jar java-app-1.0-SNAPSHOT.jar