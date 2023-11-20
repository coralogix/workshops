# Coralogix Workshops

These workshops are designed to help learn and practice observability and practice integrations to the Coralogix platform. They include reference implementations that are easy to deploy and understand.

These workshops are not documentation- Coralogix platform documentation is located [here](https://coralogix.com/docs/)

## OpenTelemetry Info  

[OpenTelemetry Overview and Value Proposition](otel/about-opentelemetry.md)  

## Workshops  

### APM - Application Perfomance Measurement Reference Examples and Training

#### Microservices
[Kubernetes (Python, Java)](otel/microservices-workshop/index.md)  
- Kubernetes OpenTelemetry Collector for metrics/traces/logs  
- OpenTelemetry tracing instrumentation for containerized Python and Java apps  
- Prometheus custom metrics collection  
- Sample "bad" deployment and errors  

[AWS ECS Fargate (Python)](otel/microservices-fargate/index.md)  
- 2x container microservice, OpenTelemetry Collector, Firelens log router  
- Complete Fargate task and ECS config instructions  

#### Monolith
Monolith Otel Collector & Host based applications  
- [Node.js / Linux](otel/monolith-workshop/node.md)  
- [Python / Linux](otel/monolith-workshop/python.md) 
- [.NET / Windows](otel/monolith-workshop/windows.md)  