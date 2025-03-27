sudo apt install -y python3.12-venv
python3 -m venv env
source env/bin/activate
pip3 install Django==5.1.7 python-json-logger
pip3 install opentelemetry-distro opentelemetry-exporter-otlp
opentelemetry-bootstrap -a install