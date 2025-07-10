#!/bin/bash
set -e
set -u
set -o pipefail

if [ "$(id -u)" -ne 0 ]; then
    echo "This script must be run as root" 1>&2
    exit 1
fi

sudo dpkg --purge opentelemetry-injector

echo "Removing OpenTelemetry Injector configuration directory..."
sudo rm -rf /etc/opentelemetry/otelinject

echo "Removing OpenTelemetry Injector library..."
sudo rm -rf /usr/lib/opentelemetry/libotelinject.so

echo "Removing OpenTelemetry Injector preload entry..."
sudo sed -i '\|/usr/lib/opentelemetry/libotelinject.so|d' /etc/ld.so.preload

echo "OpenTelemetry Injector uninstallation complete."