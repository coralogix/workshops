# requires dockerhub login
sudo docker build . -t coraexp-redis
sudo docker tag coraexp-redis public.ecr.aws/w3s4j9x9/microservices-demo:coraexp-redis
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:coraexp-redis