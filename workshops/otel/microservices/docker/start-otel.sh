sudo docker pull otel/opentelemetry-collector-contrib
sudo docker run -d \
  --name otelcol-contrib \
  --network host \
  -v "$(pwd)"/config.yaml:/etc/otelcol-contrib/config.yaml \
  -v /tmp/log/:/tmp/log/ \
  otel/opentelemetry-collector-contrib