sudo docker build . -f dotnet.dockerfile -t microsvcsdemo-dotnet && \
sudo docker tag microsvcsdemo-dotnet public.ecr.aws/w3s4j9x9/microservices-demo:dotnet
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:dotnet