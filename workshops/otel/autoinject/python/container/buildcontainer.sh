# requires dockerhub login
sudo docker build . -t autoinject-python-autogen
sudo docker tag autoinject-python-autogen public.ecr.aws/w3s4j9x9/autoinject-demo:py-autoinject
sudo docker push public.ecr.aws/w3s4j9x9/autoinject-demo:py-autoinject