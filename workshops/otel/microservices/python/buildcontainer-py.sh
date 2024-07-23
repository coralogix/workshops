# requires dockerhub login
sudo docker build . -f dockerfile-python -t microsvcsdemo-python
sudo docker tag microsvcsdemo-python public.ecr.aws/w3s4j9x9/microservices-demo:python
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:python