# requires dockerhub login
sudo docker build . -f dockerfile -t microsvcsdemo-node && \
sudo docker tag microsvcsdemo-node public.ecr.aws/w3s4j9x9/microservices-demo:node
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:node