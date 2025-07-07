#!/bin/bash

git clone https://github.com/open-telemetry/opentelemetry-injector.git
cd /opentelemetry-injector
sudo make rpm-package deb-package
sudo dpkg -i opentelemetry-injector_0.0.1-post_amd64.deb
sudo echo /usr/lib/opentelemetry/libotelinject.so >> /etc/ld.so.preload
# cd /etc/opentelemetry/otelinject
# sudo vim node.conf