# requires dockerhub login
sudo docker build . -t coraexp-mongo
sudo docker tag coraexp-mongo public.ecr.aws/w3s4j9x9/microservices-demo:coraexp-mongo
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:coraexp-mongo