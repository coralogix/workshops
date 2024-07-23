### Prometheus Auto Discovery  
  
TEMPORARILY UNDER CONSTRUCTION- DO NOT USE  
1. Update globals in `override-prometheus-auto.yaml` file
2. Deploy override and Prometheus example: `source deploy-prometheus-auto.sh`   
3. Two metrics will be visible generating random values from 0-100: `prom1` and `prom2`   
3. Examine `deploy-prometheus-auto.yaml` and understand how the Prometheus services are properly annotated  
4. Clean up: `source delete-prometheus.sh`  