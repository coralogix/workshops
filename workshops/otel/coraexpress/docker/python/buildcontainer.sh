
sudo docker build . -t coraexp-python
sudo docker tag coraexp-python public.ecr.aws/w3s4j9x9/microservices-demo:coraxp-python
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:coraxp-python