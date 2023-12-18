#!/bin/bash
pip3 install requests
pip3 install opentelemetry-distro==0.41b0 opentelemetry-exporter-otlp==1.20.0
~/.local/bin/opentelemetry-bootstrap -a install