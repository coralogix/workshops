sudo docker build . -t microsvcsdemo-go
sudo docker tag microsvcsdemo-go public.ecr.aws/w3s4j9x9/microservices-demo:go
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:go