# change the endoint env variable in the deployment .yaml to change location of your agent
# change target URL below to alter the test target
java -javaagent:/opt/opentelemetry-javaagent.jar \
-DtargetUrl=$JAVA_TEST_URL \
-Dlog4j.configurationFile=log4j2.xml \
-cp java-app-1.0-SNAPSHOT.jar demo.main.GetExample