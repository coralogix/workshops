# change the endoint env variable in the deployment .yaml to change location of your agent
# change target URL below to alter the test target
java \
-Dlog4j.configurationFile=classpath:log4j2.xml \
-jar java-app-1.0-SNAPSHOT.jar