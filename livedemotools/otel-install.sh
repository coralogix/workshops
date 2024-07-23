sudo dpkg -i ~/otelcol-contrib_0.104.0_linux_amd64.deb
sudo cp ~/config.demo.yaml /etc/otelcol-contrib/config.yaml
sudo systemctl restart otelcol-contrib.service
sudo systemctl status otelcol-contrib.service