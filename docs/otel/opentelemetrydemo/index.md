# OpenTelemetry Demo  

Based on the official [Opentelemetry Demo](https://opentelemetry.io/ecosystem/demo/)  

## Instructions


### Step 1 - Setup
Clone repo or use existing clone of it:
```
git clone https://github.com/coralogix/workshops
```  

### Step 2 - Change to workshop dir
Change to the proper directory for workshop example:  

```
cd ./workshops/workshops/otel/opentelemetrydemo
```  

### Step 3 - Update the OpenTelemetry Agent - make sure to opdate the globals first    
`source 1-deploy-override.sh` 

### Step 4 - Install the OpenTelemetry Demo and study the results      
`source 2-install-demo.sh`  

### Step 5 - Cleanup: Delete the OpenTelemetry Demo      
`source 3-delete-demo.sh`  

### Step 6 - Rollback override  
`helm rollback otel-coralogix-integration`  