# requires dockerhub login
sudo docker build . -t autoinject-java-autogen
sudo docker tag autoinject-java-autogen public.ecr.aws/w3s4j9x9/autoinject-demo:java-autoinject
sudo docker push public.ecr.aws/w3s4j9x9/autoinject-demo:java-autoinject