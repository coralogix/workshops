# Coralogix Workshops

These workshops are crafted to help you learn and practice OpenTelemetry and integrate it with the Coralogix platform. Each workshop includes easy-to-deploy reference implementations, making it simple to study and apply the concepts.

Unless specified otherwise, these workshops are designed to run on an Ubuntu host environment or Kubernetes.

Please note, these workshops are not a substitute for official Coralogix documentation. For detailed documentation, visit [Coralogix Docs](https://coralogix.com/docs/).

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

[AWS ECS Fargate (Python)](otel/ecs-fargate/index.md)
- 2x container microservice, OpenTelemetry Collector, Firelens log router
- Complete Fargate task and ECS config instructions

[AWS ECS EC2 (Python)](otel/ecs-ec2/index.md)
- OpenTelemetry collector container
- Example app container/task

### APM - Autogenerators: OpenTelemetry Tracing Instrumentation Demo Apps
Autogenerators: reference projects with live container/monolith examples
- [Autogenerators](otel/autogenerators/index.md)

### Real User Monitoring (RUM)
Visualize user experience metrics
- [Real User Monitoring (RUM)](rum/index.md) - for browsers and mobile apps

### Coralogix Live Demo Scripts
Scripts to install the Collector on a host or Kubernetes
- [Live Demo Scripts](https://github.com/coralogix/workshops/tree/master/livedemotools)
- Make sure to change all variables and refer to [Coralogix Docs](https://coralogix.com/docs/)

### Official OpenTelemetry Demo
Try out the comprehensive demo materials from the OpenTelemetry Project
- [OpenTelemetry Demo](otel/opentelemetrydemo/index.md) - for Kubernetes
