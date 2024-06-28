java -javaagent:/opt/opentelemetry-javaagent.jar \
-Dlog4j.configurationFile=src/main/resources/log4j2.xml \
-jar target/java-app-1.0-SNAPSHOT.jar
