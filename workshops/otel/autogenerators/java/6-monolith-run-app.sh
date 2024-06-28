java -javaagent:/opt/opentelemetry-javaagent.jar \
-DtargetUrl=http://api.github.com \
-Dlog4j.configurationFile=src/main/resources/log4j2.xml \
-cp target/java-app-1.0-SNAPSHOT.jar demo.main.GetExample