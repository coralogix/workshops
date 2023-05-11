# requires dockerhub login
sudo docker build . -f dockerfile-redis -t redis && \
sudo docker tag redis stevelerner/redis && \
sudo docker push stevelerner/redis