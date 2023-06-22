# requires dockerhub login
sudo docker build . -f dockerfile-microsvcsdemo -t microsvcsdemo-python && \
sudo docker tag microsvcsdemo-python public.ecr.aws/w3s4j9x9/microsvcsdemo-python:latest
sudo docker push public.ecr.aws/w3s4j9x9/microsvcsdemo-python:latest