# requires dockerhub login
sudo docker build . -t autoinject-node
sudo docker tag autoinject-node public.ecr.aws/w3s4j9x9/autoinject-demo:node-autoinject
sudo docker push public.ecr.aws/w3s4j9x9/autoinject-demo:node-autoinject