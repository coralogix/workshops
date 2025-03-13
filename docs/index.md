# Coralogix Workshops

Goal: help learn and practice OpenTelemetry and Coralogix RUM integrations  
Each workshop includes easy-to-deploy reference implementations to see exactly how these integrations work.  
  
These are designed to run on production grade cloud environments except for the RUM workshop which runs on a Mac development environment.   
Except for the RUM workshop, these are **not designed to run on a desktop or laptop machine regardless of Kubernetes/docker method of installation.**  

Before beginning please consult  [Coralogix Docs](https://coralogix.com/docs/) for any topic of interest.  
The workshops are not official documentation for the Coralogix platform.  

## Workshop Prerequisites

[Ready to get started? Check the prerequisites](prereqs.md).

## OpenTelemetry Information

Coralogix leverages open standard telemetry shippers, and infrastructure/application metrics, traces, and logs utilize OpenTelemetry.  
Learn more about the [OpenTelemetry Overview and Value Proposition](otel/about-opentelemetry.md).

## APM and OpenTelemetry Collector & Tracing Instrumentation

### Microservices Workshop

[Kubernetes / APM](otel/microservices/index.md)
- Kubernetes OpenTelemetry Collector for metrics, traces, and logs  
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

[AWS ECS Fargate (php)](otel/ecs-fargate/index.md)  
- Microservice container, OpenTelemetry Collector  
- Complete Fargate task and ECS config instructions  

[AWS ECS EC2 (Python/node)](otel/ecs-ec2/index.md)  
- OpenTelemetry collector container  
- Example app container/task and Cloudformation stack

### APM - Autogenerators: Otel Tracing Instrumentation Demo Apps  
Tracing examples for container/monolith: demos for .NET, Node, Java, and Python  
- [Autogenerators](otel/autogenerators/index.md)  

### APM - Autoinjection: Otel Tracing on K8S With Auto Instrumentation Injection
Automatically inject traciong instrumentation in k8s: demos for .NET, Node, Java, and Python  
- [Autoinjection](otel/autoinjection/index.md)  
  
### APM - eBPF: Otel APM Without Any Instrumentation - uses Linux Kernel Software based on eBPF  
Automatically generate APM spans and dashboards without any instrumentation- demos for .NET, Node, Java, and Python  
- [eBPF](otel/ebpf/index.md)  

### OpenTelmetry Manual Instrumentation   
Examples of Otel APIs/SDKs to export telemetry directly from an application   
- [Otel Manual Instrumentation](otel/manual-instrumentation/index.md)  

### Real User Monitoring (RUM)  
Visualize user experience metrics  
- [Real User Monitoring (RUM)](rum/index.md) for browsers and mobile apps  

### Coralogix Live Demo Scripts  
Scripts to install the Collector on a host or Kubernetes  
- [Live Demo Scripts](https://github.com/coralogix/workshops/tree/master/livedemotools)  
- Make sure to change all variables and refer to [Coralogix Docs](https://coralogix.com/docs/)  

### Official OpenTelemetry Demo  
Try out the comprehensive demo materials from the OpenTelemetry Project  
- [OpenTelemetry Demo](otel/opentelemetrydemo/index.md) for Kubernetes  