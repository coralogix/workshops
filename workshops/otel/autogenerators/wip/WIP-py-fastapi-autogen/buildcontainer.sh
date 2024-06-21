# requires dockerhub login
sudo docker build . -f dockerfile -t python-fastapi && \
sudo docker tag python-fastapi public.ecr.aws/w3s4j9x9/microservices-demo:py-fastapi && \
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:py-fastapi