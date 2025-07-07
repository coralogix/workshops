#!/bin/bash

git clone https://github.com/open-telemetry/opentelemetry-injector.git
cd opentelemetry-injector
sudo make rpm-package deb-package
cd instrumentation/dist
sudo dpkg -i opentelemetry-injector_0.0.1-post_amd64.deb
echo /usr/lib/opentelemetry/libotelinject.so | sudo tee -a /etc/ld.so.preload
# You can update the config values in /etc/opentelemetry/otelinject