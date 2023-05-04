# requires dockerhub login
sudo docker build . -f dockerfile-otel.python -t otel-python && \
sudo docker tag otel-python stevelernercx/otel-python && \
sudo docker push stevelernercx/otel-python