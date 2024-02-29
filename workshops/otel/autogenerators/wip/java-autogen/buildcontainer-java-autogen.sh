# requires dockerhub login
mvn clean
curl -L https://github.com/open-telemetry/opentelemetry-java-instrumentation/releases/latest/download/opentelemetry-javaagent.jar -o ./opentelemetry-javaagent.jar
mvn package
sudo docker build . -f Dockerfile -t java-autogen
sudo docker tag java-autogen public.ecr.aws/w3s4j9x9/microservices-demo:java-autogen
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:java-autogen
rm ./opentelemetry-javaagent.jar
mvn clean