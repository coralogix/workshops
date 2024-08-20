dotnet clean
dotnet build
sudo docker build . -t microsvcsdemo-dotnet8
sudo docker tag microsvcsdemo-dotnet8 public.ecr.aws/w3s4j9x9/autoinject-demo:dotnet8
sudo docker push public.ecr.aws/w3s4j9x9/autoinject-demo:dotnet8
dotnet clean