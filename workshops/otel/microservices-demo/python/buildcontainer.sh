# requires dockerhub login
sudo docker build . -f dockerfile-microsvcsdemo.python -t microsvcsdemo-python && \
sudo docker tag microsvcsdemo-python stevelerner/microsvcsdemo-python
sudo docker push stevelerner/microsvcsdemo-python