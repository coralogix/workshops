# requires dockerhub login
sudo docker build . -f dockerfile-microsvcsdemo -t microsvcsdemo-node && \
sudo docker tag microsvcsdemo-node public.ecr.aws/w3s4j9x9/demo-node-autogen:latest
sudo docker push public.ecr.aws/w3s4j9x9/demo-node-autogen:latest