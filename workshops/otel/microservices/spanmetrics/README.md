## Spanmetrics demo for the Microservices Workshop  
  
### Step 1: Deploy override  

Update `override-spanmetrics.yaml` with your cluster before deploying:    
```
source deploy-spanmetrics.sh
```
  
### Step 2: Build promQL query  
  
Sample custom dashboard promQL:

```
avg_over_time(calls_total{service_name="cx-payment-gateway-flask", span_name="handle_request", status_code="STATUS_CODE_ERROR"}[1m])
```

Full documentation here: [https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/connector/spanmetricsconnector](https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/connector/spanmetricsconnector)
