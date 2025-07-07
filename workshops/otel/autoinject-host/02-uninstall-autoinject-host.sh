#!/bin/bash

sudo dpkg -r opentelemetry-injector
sudo rm -rf /etc/opentelemetry/otelinject
sudo rm -rf /usr/lib/opentelemetry/libotelinject.so