# OpenTelemetry for Log Observability

## Instructions

This example is for basic study only and is not documentation.    
Full documentation: [https://coralogix.com/docs/](https://coralogix.com/docs/)  
Requirements:  
- Coralogix Acccount  
- Cloud Linux host (Debian preferred but RPM flavors will work)  
- Updated versions and sufficient permissions for installing software  
- Proper IDE i.e. Visual Studio Code 


### Step 1 - Setup
Clone repo:
```
git clone https://github.com/coralogix/workshops
```  

### Step 2 - Change to workshop dir
Change to the proper directory for workshop example:  

```
cd ./workshops/workshops/otel/logparsing/
```  

### Step 3 - Set up Otel Collector on a Linux host     
Download and install latest CONTRIB release version from here:  
[https://github.com/open-telemetry/opentelemetry-collector-releases/releases](https://github.com/open-telemetry/opentelemetry-collector-releases/releases)  

Collector `config.yaml` must be configured with Coralogix Exporter. See the "Send Data to Coralogix" section for determining telemetry endpoint and and API key: [https://coralogix.com/docs/guide-first-steps-coralogix/](https://coralogix.com/docs/guide-first-steps-coralogix/)    

See the `config.yaml` example in `./workshops/workshops/otel/logparsing/otelcol`  
This file can be updated in `/etc/otelcol/contrib/config.yaml` - make a backup of the default version first.    
  
Manage the collector using `systemctl` i.e. `sudo systemctl restart otelcol`  
More info is here: [https://coralogix.com/docs/guide-first-steps-coralogix/](https://coralogix.com/docs/guide-first-steps-coralogix/)  
  
You can check Collector status with these status URLS:  
`http://localhost:55679/debug/tracez`  
`http://localhost:55679/debug/pipelinez`    
For more info: [zpages docs](https://github.com/open-telemetry/opentelemetry-collector/blob/main/extension/zpagesextension/README.md)  


### Step 4 - Prepare Log Example 

- Copy the LOG.log.gz file to `/tmp/cx`  
- `gunzip Log.log.gz`  

### Step 5 - Update Collector with Log Receiver  
  
- Add the following receiver to `/etc/otelcol/config.yaml`  
```
receivers:
  filelog:
    include: [ /tmp/cx/LOG.log ]
```  
- Add the following pipeline to `/etc/otelcol/config.yaml`  
```
service:  
  pipelines:
    logs:
      receivers: [otlp, filelog]
```
- Restart the collector:
```
sudo systemctl restart otelcol-contrib.service 
```  
- Check collector status:
```
sudo systemctl status otelcol-contrib.service 
```  
### Step 5 - Study Logs in Coralogix
  
The `Explore->Logs` option now will show the integrated logs.  