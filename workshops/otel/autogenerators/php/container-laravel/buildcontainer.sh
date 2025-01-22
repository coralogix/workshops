# requires dockerhub login
sudo docker build . -t php-laravel
sudo docker tag php-laravel public.ecr.aws/w3s4j9x9/microservices-demo:php-laravel
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:php-laravel