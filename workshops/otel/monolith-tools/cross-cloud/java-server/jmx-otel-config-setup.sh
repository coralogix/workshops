rm -rf /opt/opentelemetry-java-contrib-jmx-metrics.jar
sudo curl -L https://github.com/open-telemetry/opentelemetry-java-contrib/releases/download/v1.37.0/opentelemetry-jmx-metrics.jar -o /opt/opentelemetry-java-contrib-jmx-metrics.jar
sudo chmod 644 /opt/opentelemetry-java-contrib-jmx-metrics.jar
sudo cp config-demo-jmx.yaml /etc/otelcol-contrib/config.yaml
sudo systemctl restart otelcol-contrib.service
sudo systemctl status otelcol-contrib.service