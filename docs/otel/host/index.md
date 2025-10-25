# OpenTelemetry Collector For HOsts

## Instructions

### Linux

This example is for Ubuntu / Debian. Adjust accordingly for your Linux distro.  


### Step 1 - Download Collector
Download the appropriate collector for your Linux OS / Architecture (including Mac) here:

[OpenTelemetry Collector Releases](https://github.com/open-telemetry/opentelemetry-collector-releases/releases)

### Step 2 - Change to workshop dir
Change to the proper directory for workshop example:  

```
cd ./workshops/workshops/otel/host
```  

### Step 3 - Update host config with your Coralogix team and key  

Update `config.yaml` accordingly.
  
### Step 4 - Install Collector
Study this script and make it work for you and then execute it.  
This is only an example of installing a collector and copying a premade config.  

Example install script is here:
```
source host-install-otel.sh
``` 

### Step 5 - View Host in Infrastructure Explorer
Your host should now be visible in Coralogix Infrastructure Explorer  

Optional- upload `host_dashboard.json` to Custom Dashboards for a customizable view of host metrics.  
  
### Step 6 - Delete Collector
  
Study this script and make it work for you and then execute it.  
This is only an example of installing a collector and copying a premade config.  
  
Example install script is here:
```
source host-delete-otel.sh
``` 