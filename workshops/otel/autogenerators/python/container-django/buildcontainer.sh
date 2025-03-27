# requires dockerhub login
sudo docker build . -t python-autogen-django
sudo docker tag python-autogen-django public.ecr.aws/w3s4j9x9/microservices-demo:py-autogen-django
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:py-autogen-django