## Docker-On-Host Example  

Requires a host with docker installed and full perms for host network and filesystem access  
   
### Step 1 - Update collector config
- Update the exporter key/domain/application/subsystem in: `config.yaml` 
  
### Step 2 - Start Otel Collector Docker Container
- `source start-otel.sh`  
  
### Step 3 - Start Python Trace/Log Generator Container 
- `source start-autogen.sh` 
  
### Step 4 - Study examples  
`config.yaml` shows how collector is installed   
`start-autogen.sh` shows how a Dockerized app with OpenTelemetry tracing instrumentation is configured  

### Cleanup
- `source stop-all.sh`  