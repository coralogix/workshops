sudo docker build . -t microsvcsdemo-dotnet6
sudo docker tag microsvcsdemo-dotnet6 public.ecr.aws/w3s4j9x9/microservices-demo:dotnet6
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:dotnet6