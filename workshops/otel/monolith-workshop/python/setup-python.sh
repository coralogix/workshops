#!/bin/bash
pip3 install requests
pip3 install opentelemetry-distro 
pip3 install opentelemetry-exporter-otlp 
pip3 install opentelemetry-instrumentation-requests
~/.local/bin/opentelemetry-bootstrap