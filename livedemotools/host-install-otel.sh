sudo dpkg -i [your downloaded version of otel here]
sudo cp ~/config.demo.yaml /etc/otelcol-contrib/config.yaml
# copy the config file to the home directory for easy access
sudo cp ~/config.demo.yaml ~/config.yaml
sudo systemctl restart otelcol-contrib.service
sudo systemctl status otelcol-contrib.service