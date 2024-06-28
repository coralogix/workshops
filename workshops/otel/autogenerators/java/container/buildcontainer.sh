# requires dockerhub login
sudo docker build . -t microsvcsdemo-java-autogen
sudo docker tag microsvcsdemo-java-autogen public.ecr.aws/w3s4j9x9/microservices-demo:java-autogen
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:java-autogen