sudo apt install -y python3.12-venv
python3 -m venv env
source env/bin/activate
# pip3 install opentelemetry-distro opentelemetry-exporter-otlp
pip3 install \
  opentelemetry-api \
  opentelemetry-sdk \
  opentelemetry-exporter-otlp-proto-grpc