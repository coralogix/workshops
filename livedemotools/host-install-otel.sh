sudo dpkg -i [your downloaded version of otel here]
sudo cp ~/config.demo.yaml /etc/otelcol-contrib/config.yaml
sudo systemctl restart otelcol-contrib.service
sudo systemctl status otelcol-contrib.service