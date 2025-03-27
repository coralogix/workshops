# requires dockerhub login
sudo docker build . -t python-autogen-jaeger
sudo docker tag python-autogen-jaeger public.ecr.aws/w3s4j9x9/microservices-demo:py-autogen-jaeger
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:py-autogen-jaeger