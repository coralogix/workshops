# requires dockerhub login
sudo docker build . -f dockerfile -t mysql && \
sudo docker tag mysql public.ecr.aws/w3s4j9x9/microservices-demo:mysql
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:mysql