sudo dpkg -i ~/otelcol*
sudo cp ~/config.yaml /etc/otelcol-contrib
sudo systemctl restart otelcol-contrib.service
sudo systemctl status otelcol-contrib.service