# Coralogix Workshops

These workshops are designed to help learn and practice Opentelemetry and its integration with the Coralogix platform.  
They include reference implementations that are easy to deploy and study.    

Unless otherwise specified, these are designed to run on an Ubuntu host environment or Kubernetes.  

These workshops are not documentation- Coralogix  documentation is located [here](https://coralogix.com/docs/)
  
## Workshop Prerequisites  

[Ready to do try a workshop? Here are the prerequisites](prereqs.md)  
  
## OpenTelemetry Info  
Coralogix uses open standard telemetry shippers and infrastructure/application metrics/traces/logs make use of OpenTelmetry    
[OpenTelemetry Overview and Value Proposition](otel/about-opentelemetry.md)  
  
## APM and OpenTelemetry Collector and Tracing Instrumentation  

### Microservices Workshop  

[Kubernetes / APM ](otel/microservices/index.md)  
- Kubernetes OpenTelemetry Collector for metrics/traces/logs   
- OpenTelemetry tracing instrumentation for containerized Python apps  
- Simulated application using real microservices  
- Prometheus custom metrics collection   
- Redis  
- Sample "bad" deployment and errors  

**OpenTelemetry Collector Configuration Examples**  
- [Prometheus](otel/prometheus/index.md)  
- [MySQL Metrics + Query Performance](otel/mysql/index.md)  
- [Redis Metrics](otel/redis/index.md)  

### Other Containerized Environments  
  
[AWS ECS Fargate (Python)](otel/ecs-fargate/index.md)  
- 2x container microservice, OpenTelemetry Collector, Firelens log router  
- Complete Fargate task and ECS config instructions  
  
[AWS ECS EC2 (Python)](otel/ecs-ec2/index.md)  
- OpenTelemetry collector container  
- Example app container/task  

### APM - OpenTelemetry Tracing Instrumentation Reference Projects for Containers and Monoliths   
Autogenerators: reference projects with live container/monolith examples  
- [Autogenerators](otel/autogenerators/index.md)   
  
### Real User Monitoring (RUM)
Visualize user experience metrics  
- [Real User Monitoring (RUM)](rum/index.md) - requires only a web browser

### Official OpenTelemetry Demo
Try out the comprehensive demo materials from the OpenTelemetry Project  
- [OpenTelemetry Demo](otel/opentelemetrydemo/index.md) - for k8s  