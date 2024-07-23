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
cd ./workshops/workshops/otel/microservices/mysql
```  

### Step A - Deploy the example MySQL database deployment and SQL query app  
In: `mysql/1-mysql-deployment`  
`source deploy-mysqld.sh`  

In: `mysql/2-mysqlqueryapp`  
`source deploy-mysqlquery.sh` 
  
### Step B - Deploy the Otel Collector [MySQL Receiver](https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/receiver/mysqlreceiver)   
In `mysql/3-mysqlreceiver`  
Update the global variables first and then:  
`source deploy-override.sh`  

### Step C - Deploy the Otel [SQL Query Receiver](https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/receiver/sqlqueryreceiver)    
In `mysql/4-sqlqueryreceiver`  
Update the global variables first and then:  
`source deploy-override.sh`  
  
### Step 6 - Upload dashboard example to Custom Dashboards
*Coralogix->Dashboards->Custom Dashboards->New->Import*  
  
Import: `mysql/dashboard-mysql.json`  

### Step 7 - Study deployment examples

`mysqlreceiver` - override yaml shows the MySQL receiver configuration example  
  
`sqlqueryreceiver` - override yaml shows the SQL synthetic query configuration example  

Observability samples of both are shown on the MySQL Custom Dashboard  
  
### Step 8 - Cleanup

Roll back to remove receivers:  
`helm rollback otel-coralogix-integration 1` (or your proper previous version) 
  
Delete the sample MySQL Database:  
In: `mysql/1-mysql-deployment`  
`source delete-mysqld.sh`   
In: `mysql/2-mysqlqueryapp`  
`source delete-mysqlquery.sh`   