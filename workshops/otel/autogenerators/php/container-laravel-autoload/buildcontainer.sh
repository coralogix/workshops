# requires dockerhub login
sudo docker build . -t php-laravel-autogen
sudo docker tag php-laravel-autogen public.ecr.aws/w3s4j9x9/microservices-demo:php-laravel-autogen
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:php-laravel-autogen