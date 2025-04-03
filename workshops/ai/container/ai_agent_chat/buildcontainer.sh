# requires dockerhub login
sudo docker build . -t py-ai-chat
sudo docker tag py-ai-chat public.ecr.aws/w3s4j9x9/microservices-demo:py-ai-chat
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:py-ai-chat