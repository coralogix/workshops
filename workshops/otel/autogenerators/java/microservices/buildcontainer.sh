# requires dockerhub login
sudo docker build . -t java-microservices-autogen
sudo docker tag java-microservices-autogen public.ecr.aws/w3s4j9x9/microservices-demo:java-microservices-autogen
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:java-microservices-autogen