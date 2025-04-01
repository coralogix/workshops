aws ecr-public get-login-password --region us-east-1 | sudo sudo docker login --username AWS --password-stdin public.ecr.aws
sudo docker build -t cx-ruby-logs-metrics .
sudo docker tag cx-ruby-logs-metrics public.ecr.aws/w3s4j9x9/microservices-demo:cx-ruby-logs-metrics
sudo docker push public.ecr.aws/w3s4j9x9/microservices-demo:cx-ruby-logs-metrics