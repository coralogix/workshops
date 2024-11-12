# requires dockerhub login
sudo docker build . -t php-autogen
sudo docker tag php-autogen public.ecr.aws/w3s4j9x9/microservices-demo:php-autogen-http
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:php-autogen-http