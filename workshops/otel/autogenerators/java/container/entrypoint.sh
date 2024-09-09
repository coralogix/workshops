# change the endoint env variable in the deployment .yaml to change location of your agent
# change target URL below to alter the test target
java -javaagent:/opt/opentelemetry-javaagent.jar \
-Dlog4j.configurationFile=classpath:log4j2.xml \
-Dcom.sun.management.jmxremote \
-Dcom.sun.management.jmxremote.port=9999 \
-Dcom.sun.management.jmxremote.authenticate=false \
-Dcom.sun.management.jmxremote.ssl=false \
-jar java-app-1.0-SNAPSHOT.jar