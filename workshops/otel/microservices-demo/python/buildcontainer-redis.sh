# requires dockerhub login
sudo docker build . -f dockerfile-redis -t redis && \
sudo docker tag redis public.ecr.aws/w3s4j9x9/redis:latest && \
sudo docker push public.ecr.aws/w3s4j9x9/redis:latest