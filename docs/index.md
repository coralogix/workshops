# Coralogix Workshops

Coralogix workshops are community maintained reference implementations and tutorials on a variety of topics including  
- OpenTelemetry tracing / Collector configs  
- Observability techniques  
- Coralogix platform  
    
Except for the RUM workshops, the workshops are **not designed to run on a desktop or laptop machine regardless of Kubernetes/docker method of installation.**  They are only designed to run on production grade Kubernetes clusters or virtual machines i.e. AWS EKS or EC2.  

The workshops are community maintained only and are not official documentation for either OpenTelemetry or Coralogix  
Official docs are here:  
[Coralogix Docs](https://coralogix.com/docs/)  
[OpenTelemetry](https://opentelemetry.io/)  

## Workshop Prerequisites

[Ready to get started? Check the prerequisites](prereqs.md)

## Coralogix and OpenTelemetry  

Learn more about the [Coralogix and OpenTelemetry here](https://coralogix.com/docs/opentelemetry/getting-started/)

# The Workshops
Workshops are divided into three types: 
- OpenTelemetry Collector: various configuration techniques  
- Tracing Instrumentation: multiple types of instrumentating apps to emit traces
- Other: Real User Monitoring (Browser/Mobile) and other examples

They all operate the same way:  
- Each example has a reference app and/or config you can try  
- Study the Github repo contents to understand how these configurations/examples work  


## Tracing Workshops

[Microsservices Workshop: Tracing + Collector configs](otel/microservices/index.md)
- Kubernetes OpenTelemetry Collector for metrics, traces, and logs  
- OpenTelemetry tracing instrumentation for containerized Python apps  
- Simulated application using real microservices  
- Prometheus custom metrics collection  
- Redis trace spans / database monitoring 
- Sample "bad" deployment and errors  

**OpenTelemetry Collector Configuration Examples**  
- [Prometheus](otel/prometheus/index.md)  
- [MySQL Metrics + Query Performance](otel/mysql/index.md)  
- [Redis Metrics](otel/redis/index.md)  

## Tracing In Hosted Container Environments

[AWS ECS Fargate (php)](otel/ecs-fargate/index.md)  
- Microservice container, OpenTelemetry Collector  
- Complete Fargate task and ECS config instructions  

[AWS ECS EC2 (Python/node)](otel/ecs-ec2/index.md)  
- OpenTelemetry collector container  
- Example app container/task and Cloudformation stack

## Tracing Instrumentation Examples For Hosts/Containers in Various Languages  
### Autogenerators: Otel Tracing Instrumentation Demo Apps  
Tracing examples for container/monolith: demos for .NET, Node, Java, and Python  
- [Autogenerators](otel/autogenerators/index.md)  

## Kubernetes Specific Easy Tracing  

### OpenTelemetry Autoinjection: Otel Tracing on K8S With Auto Instrumentation Injection
Automatically inject traciong instrumentation in k8s: demos for .NET, Node, Java, and Python  
- [Kubernetese Instrumentation Autoinjection](otel/autoinjection/index.md)  

### eBPF: Otel APM Without Any Instrumentation - uses Linux Kernel Software based on eBPF
Automatically generate APM spans and dashboards without any instrumentation- demos of .NET, Node, Java, and Python  
- [Kubernetes eBPF](otel/ebpf/index.md)  


## Manual Instrumetnation 

### OpenTelmetry Manual Instrumentation
Examples of Otel APIs/SDKs to export logs/metrics/trace telemetry directly from an application to an OpenTelemetry Collector or OTLP endpoint  
- [Otel Manual Instrumentation](otel/manual-instrumentation/index.md)  

## Real User Monitoring (RUM)  
Visualize user experience metrics and link user sessions to back end applications  
- [Real User Monitoring (RUM)](rum/index.md) for browsers and mobile apps  

## Coralogix Live Demo Scripts  
Scripts to install the Collector on a host or Kubernetes  
- [Live Demo Scripts](https://github.com/coralogix/workshops/tree/master/livedemotools)  
- Make sure to change all variables and refer to [Coralogix Docs](https://coralogix.com/docs/)  
  
## Official OpenTelemetry Demo  
Try out the comprehensive demo materials from the OpenTelemetry Project  
- [OpenTelemetry Demo](otel/opentelemetrydemo/index.md) for Kubernetes  