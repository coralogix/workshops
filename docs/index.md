# Coralogix Workshops

These workshops are designed to help learn and practice observability and practice integrations to the Coralogix platform.  
They include reference implementations that are easy to deploy and study.    

Unless otherwise specified, these are designed to run on an Ubuntu cloud host environment.   

These workshops are not documentation- Coralogix  documentation is located [here](https://coralogix.com/docs/)
  
## Workshop Prerequisites  

[Ready to do try a workshop? Here are the prerequisites](prereqs.md)  
  

## OpenTelemetry Info  
Coralogix uses open standard telemetry shippers and infrastructure/application metrics/traces/logs make use of OpenTelmetry    
[OpenTelemetry Overview and Value Proposition](otel/about-opentelemetry.md)  
  
## Workshops  
  
### Microservices and OpenTelemetry Collector Configurations
[Kubernetes / APM ](otel/microservices/index.md)  
- Kubernetes OpenTelemetry Collector for metrics/traces/logs  
- OpenTelemetry tracing instrumentation for containerized Python and Java apps  
- Prometheus custom metrics collection  
- MySQL 
- Redis  
- Sample "bad" deployment and errors  

After completing the Microservices Workshop: OpenTelemetry Collector Configuration  
- [Prometheus](otel/prometheus/index.md)  
- [MySQL Metrics + Query Performance](otel/mysql/index.md)  
- [Redis Metrics](otel/redis/index.md)  
  
[AWS ECS Fargate (Python)](otel/ecs-fargate/index.md)  
- 2x container microservice, OpenTelemetry Collector, Firelens log router  
- Complete Fargate task and ECS config instructions  
  
[AWS ECS EC2 (Python)](otel/ecs-ec2/index.md)  
- OpenTelemetry collector container  
- Example app container/task  
  
### APM - Monolith
Monolith Opentelemetry Collector and tracing instrumentation  
- [Python / Linux](otel/monolith/python.md)   
- [Node / Linux](otel/monolith/node.md)  
- [.NET / Windows](otel/monolith/windows.md)  

### Real User Monitoring (RUM)
Visualize user experience metrics  
- [Real User Monitoring (RUM)](rum/index.md) - requires only a web browser