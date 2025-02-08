aws ecr-public get-login-password --region us-east-1 | sudo docker login --username AWS --password-stdin public.ecr.aws
sudo docker build . -t cx-autogen-go
sudo docker tag cx-autogen-go public.ecr.aws/w3s4j9x9/microservices-demo:go-autogen
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:go-autogen