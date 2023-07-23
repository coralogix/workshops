# requires dockerhub login
sudo docker build . -f dockerfile-inventory -t inventory-python && \
sudo docker tag inventory-python public.ecr.aws/w3s4j9x9/utilities:python-inventory
sudo docker push public.ecr.aws/w3s4j9x9/utilities:python-inventory