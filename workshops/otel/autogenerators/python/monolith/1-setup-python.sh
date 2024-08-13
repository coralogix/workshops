sudo apt install -y python3.12-venv
python3 -m venv env
source env/bin/activate
pip3 install requests Flask
# pip3 install opentelemetry-distro opentelemetry-exporter-otlp
pip3 install opentelemetry-distro==0.46b0 opentelemetry-exporter-otlp==1.25.0
opentelemetry-bootstrap -a install