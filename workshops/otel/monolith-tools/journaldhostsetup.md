## setup notes for hosts

for Ubuntu:
```
wget https://github.com/open-telemetry/opentelemetry-collector-releases/releases/download/v0.99.0/otelcol-contrib_0.99.0_linux_amd64.deb
```
ubuntu: sudo dpkg -i o[tab]
aws: sudo rpm -i o[tab]

```
sudo usermod -a -G systemd-journal otelcol-contrib
sudo usermod -a -G adm otelcol-contrib
sudo chown -R otelcol-contrib /var/log/journal/
sudo chmod -R 750 /var/log/journal/
vi /etc/default/grub GRUB_CMDLINE_LINUX_DEFAULT="quiet splash apparmor=0"
sudo update-grub
restart
```
```
sudo systemctl status otelcol-contrib.service
```
```
sudo cp /etc/otelcol-contrib/config.yaml /etc/otelcol-contrib/config.yaml.orig
```
  
[optional to work from original config] `cp /etc/otelcol-contrib/config.yaml ./config.demo.yaml`  

Add hostmetrics receiver 10s: `https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/receiver/hostmetricsreceiver`

Add journald receiver - check services first: `systemctl list-units --type=service --state=active | grep ".service" | awk '{print $1}'`  
Have chatgpt add "- " in front into copyable code format  

Add filelog receiver `https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/exporter/coralogixexporter`

Get demo example from workshop and configure it  
```
sudo cp ~/config.demo.yaml /etc/otelcol-contrib/config.yaml
```

sudo systemctl restart otelcol[tab]  
sudo systemctl status otelcol[tab]  

coralogix->deploy integrations: opentelemetry + hostmetrics (uploaded)

cleanup ubuntu: sudo dpkg -r otelcol-contrib