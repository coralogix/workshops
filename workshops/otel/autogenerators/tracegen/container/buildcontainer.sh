# requires dockerhub login
sudo docker build . -t python-tracegen
sudo docker tag python-tracegen public.ecr.aws/w3s4j9x9/microservices-demo:py-tracegen
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:py-tracegen