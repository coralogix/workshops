sudo docker build . -f dockerfile-php -t microservices-demo:php-demo
sudo docker tag microservices-demo:php-demo public.ecr.aws/w3s4j9x9/php-demo:0.1.0
sudo docker push public.ecr.aws/w3s4j9x9/php-demo:0.1.0