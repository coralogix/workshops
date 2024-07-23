# OpenTelemetry Collector Configuration for redis

## Instructions

Complete the Microservices Workshop and leve the example running

### Step 1 - Change to workshop dir
Change to the proper directory for workshop example:  

```
cd ./workshops/workshops/otel/microservices
```  

### Step 2 - Deploy the Otel Collector Redis Receiver 
In: `redis/redis`  
`source deploy-redis.sh` 

### Step 3 - Upload dashboard example to Custom Dashboards
*Coralogix->Dashboards->Custom Dashboards->New->Import*  
  
Import: `redis/dashboard.json`  

### Step 4 - Study deployment examples

`redisreceiver` - override yaml shows the redis receiver configuration example  

Observability samples of both are shown on the redis Custom Dashboard  
  
### Cleanup

Roll back to remove receivers:  
`helm rollback otel-coralogix-integration 1` (or your proper previous version)