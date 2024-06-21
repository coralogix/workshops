# requires dockerhub login
sudo docker build . -f dockerfile -t python-autogen && \
sudo docker tag python-autogen public.ecr.aws/w3s4j9x9/microservices-demo:py-autogen && \
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:py-autogen