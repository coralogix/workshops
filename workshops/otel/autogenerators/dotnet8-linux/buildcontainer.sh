sudo docker build . -f Dockerfile -t microsvcsdemo-dotnet8 && \
sudo docker tag microsvcsdemo-dotnet8 public.ecr.aws/w3s4j9x9/microservices-demo:dotnet8
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:dotnet8