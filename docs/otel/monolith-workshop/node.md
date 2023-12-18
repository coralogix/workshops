# OpenTelemetry for Node.js / Monolith

## Instructions

This example is for basic study only and is not documentation.    
Full documentation: [https://coralogix.com/docs/](https://coralogix.com/docs/)  
Requirements:  
- Coralogix Acccount  
- Cloud Linux host (Debian preferred but RPM flavors will work)  
- Updated versions and sufficient permissions for installing software  
- Updated verson of `node` and `npm` installed [https://nodejs.org/](https://nodejs.org/)  
- Proper IDE i.e. Visual Studio Code 
- Ensure that the Node version is current (20 or higher).  See [https://deb.nodesource.com/](https://deb.nodesource.com/) for Ubuntu installation instructions.   

### Step 1 - Setup
Clone repo:
```
git clone https://github.com/coralogix/workshops
```  

### Step 2 - Change to workshop dir
Change to the proper directory for workshop example:  

```
cd ./workshops/workshops/otel/monolith-workshop/node/
```  

### Step 3 - Set up Otel Collector on a Linux host     
Download and install latest CONTRIB release version from here:  
[https://github.com/open-telemetry/opentelemetry-collector-releases/releases](https://github.com/open-telemetry/opentelemetry-collector-releases/releases)  

Collector `config.yaml` must be configured with Coralogix Exporter. See the "Send Data to Coralogix" section for determining telemetry endpoint and and API key: [https://coralogix.com/docs/guide-first-steps-coralogix/](https://coralogix.com/docs/guide-first-steps-coralogix/)    

See the `config.yaml` example in `./workshops/workshops/otel/monolith-workshop/node/otelcol`  
This file can be updated in `/etc/otelcol/contrib/config.yaml` - make a backup of the default version first.    
  
Manage the collector using `systemctl` i.e. `sudo systemctl restart otelcol`  
More info is here: [https://coralogix.com/docs/guide-first-steps-coralogix/](https://coralogix.com/docs/guide-first-steps-coralogix/)  
  
You can check Collector status with these status URLS:  
`http://localhost:55679/debug/tracez`  
`http://localhost:55679/debug/pipelinez`    
For more info: [zpages docs](https://github.com/open-telemetry/opentelemetry-collector/blob/main/extension/zpagesextension/README.md)  


### Step 4 - Run Node client in new terminal  

Demo app will make 250 requests of `https://api.github.com` and then exit.  

in `./workshops/workshops/otel/monolith-workshop/node/` 

Install node.js OpenTelemetry Instrumentation:  
```
setup-node.sh
```  

Setup environment variables for OpenTelemetry:
```
setup-node-env.sh
```  

Start demo app:  
```
start-node.sh
```  

### Step 5 - Study results in Coralogix portal

The shell scripts show how a monolith is set up and instrumented for OpenTelemetry.