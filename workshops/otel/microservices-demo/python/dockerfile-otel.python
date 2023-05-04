FROM python:slim
RUN mkdir /home/code
WORKDIR /home/code
COPY . /home/code/
RUN apt-get update && \
    export PATH="$HOME/.local/bin:$PATH" && \
    apt install -y python3-pip && \
    python3 -m pip install -r requirements.txt && \
    python3 -m pip install opentelemetry-distro opentelemetry-instrumentation opentelemetry-exporter-otlp && \
    opentelemetry-bootstrap -a install && \
    apt install -y curl && \
    apt -y autoremove && apt-get -y autoclean
EXPOSE 5000 9090

# apt install -y dnsutils && \
# apt install -y util-linux && \
# apt install -y coreutils && \
