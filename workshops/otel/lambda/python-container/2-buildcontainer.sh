sudo docker build . --platform linux/amd64 -t lambda-container-python && \
sudo docker tag lambda-container-python:latest 104013952213.dkr.ecr.us-west-2.amazonaws.com/lambda-container-python:latest && \
sudo docker push 104013952213.dkr.ecr.us-west-2.amazonaws.com/lambda-container-python:latest