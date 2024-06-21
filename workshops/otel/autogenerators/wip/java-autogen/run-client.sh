# change the endoint env variable in the deployment .yaml to change location of your agent
# change target URL below to alter the test target
java \
-DtargetURL=$JAVA_TEST_URL \
-javaagent:./opentelemetry-javaagent.jar \
-jar ./java-app-1.0-SNAPSHOT.jar