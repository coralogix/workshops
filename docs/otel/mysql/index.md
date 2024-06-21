# OpenTelemetry Collector Configuration for MySQL

## Instructions

Complete the Microservices Workshop or install the OpenTelemetry Collector on your k8s cluster  
   
[Easy Coralogix instructions for Complete Observability are here](https://coralogix.com/docs/otel-collector-for-k8s/)  

### Step 1 - Setup
Clone repo or use existing clone of it:
```
git clone https://github.com/coralogix/workshops
```  

### Step 2 - Change to workshop dir
Change to the proper directory for workshop example:  

```
cd ./workshops/workshops/otel/microservices
```  

### Step 3 - Deploy the example MySQL database deployment
In: `mysql/mysqld`  
`source deploy-mysqld.sh` 

### Step 4 - Deploy the Otel Collector MySQL Receiver  
In `mysql/mysqlreceiver`  
`source deploy-override.sh`  

### Step 5 - Deploy the Otel SQL Query receiver Example  
In `mysql/mysqlqueryreceiver`  
`source deploy-override.sh`  

### Step 6 - Upload dashboard example to Custom Dashboards
*Coralogix->Dashboards->Custom Dashboards->New->Import*  
  
Import: `mysql/dashboard.json`  

### Step 7 - Study deployment examples

`mysqlreceiver` - override yaml shows the MySQL receiver configuration example  
  
`sqlqueryreceiver` - override yaml shows the SQL synthetic query configuration example  

Observability samples of both are shown on the MySQL Custom Dashboard  
  
### Step 8 - Cleanup

Roll back to remove receivers:  
`helm rollback otel-coralogix-integration 1` (or your proper previous version) 
  
Delete the sample MySQL Database:  
In: `mysql/mysqld`  
`source delete-mysqld.sh`   