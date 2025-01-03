# requires dockerhub login
sudo docker build . -t coraexp-postgres
sudo docker tag coraexp-postgres public.ecr.aws/w3s4j9x9/microservices-demo:coraexp-postgres
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:coraexp-postgres