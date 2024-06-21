# requires dockerhub login
curl -L https://github.com/open-telemetry/opentelemetry-java-instrumentation/releases/latest/download/opentelemetry-javaagent.jar -o ./opentelemetry-javaagent.jar
sudo docker build . -f dockerfile-java -t otel-java
sudo docker tag microsvcsdemo-java-autogen public.ecr.aws/w3s4j9x9/microservices-demo:java-autogen
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:java-autogen
rm ./opentelemetry-javaagent.jar