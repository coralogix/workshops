dotnet clean
dotnet build
sudo docker build . -t autoinject-dotnet8
sudo docker tag autoinject-dotnet8 public.ecr.aws/w3s4j9x9/autoinject-demo:dotnet8
sudo docker push public.ecr.aws/w3s4j9x9/autoinject-demo:dotnet8
dotnet clean