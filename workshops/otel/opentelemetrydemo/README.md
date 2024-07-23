## Requires a valid Coralogix installation of full OpenTelemetry Collector for k8s  
  
### Step 1 - Override the Collector
- Update the globals to override the Collector  
- `source deploy-override.sh`  
  
### Step 2 - Install OpenTelmetry Demo
- `source install-demo.sh`   
  
### Cleanup   
- Remove Otel Demo: `source delete-demo.sh` 
- Remove override:  `helm rollback otel-coralogix-integration 1 (or your previous chart version)`
