# requires dockerhub login
sudo docker build . -t java-autogen-openjdk8
sudo docker tag java-autogen-openjdk8 public.ecr.aws/w3s4j9x9/microservices-demo:java-autogen-openjdk8
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:java-autogen-openjdk8