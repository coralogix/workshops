# Coralogix Workshops

Welcome to the **Coralogix Workshops** — community-maintained reference implementations and tutorials focused on:

- OpenTelemetry tracing and Collector configuration
- Observability techniques
- Coralogix platform usage

> **Note:** Except for RUM workshops, these examples are **not designed** to run on local machines (laptops/desktops), even with Docker or Kubernetes. They require **production-grade environments**, such as AWS EKS or EC2.

These workshops are **community-supported** and are *not official documentation* for either OpenTelemetry or Coralogix.

**Official Documentation:**

- [Coralogix Docs](https://coralogix.com/docs/)
- [OpenTelemetry Docs](https://opentelemetry.io/)

---

## Workshop Prerequisites

Before diving in, make sure you meet all requirements:  
[Check the prerequisites](prereqs.md)

---

## Coralogix and OpenTelemetry

Learn more: [Coralogix + OpenTelemetry Overview](https://coralogix.com/docs/opentelemetry/getting-started/)

---

## OpenTelemetry Collector Tools  

- OTTL Playground - test OTTL statements: [https://ottl.run/](https://ottl.run/)  
- OTelBin: visualize and share OpenTelemetry colelctor configs: [https://www.otelbin.io/](https://www.otelbin.io/)  
- Otail: validate tail sampling processor configs: [https://app.otail.dev/](https://app.otail.dev/)  

---

## The Workshops

Workshops are organized into three categories:

- **OpenTelemetry Collector** – Configuration techniques and examples
- **Tracing Instrumentation** – Sample apps instrumented in multiple languages
- **Other** – RUM (Real User Monitoring), eBPF, and more

Each workshop includes a reference app and/or configuration you can study and deploy.  
Browse the GitHub repo to understand how these examples work.

---

## Tracing Workshops

### [Microservices Workshop: Tracing + Collector Configs](otel/microservices/index.md)

Includes:

- OpenTelemetry Collector for metrics, traces, and logs (Kubernetes)
- Automatic tracing instrumentation for Python container apps
- Realistic microservices-based simulation
- Prometheus custom metrics
- Redis span tracing and DB monitoring
- Simulated failure scenarios and “bad deployments”

### OpenTelemetry Collector Configuration Examples

- [Prometheus Collector Config](otel/prometheus/index.md)
- [MySQL Metrics + Query Performance](otel/mysql/index.md)
- [Redis Metrics](otel/redis/index.md)
- [Tail Sampling](otel/tailsampling/index.md)

---

## Tracing in Hosted Container Environments

### [AWS ECS Fargate (PHP)](otel/ecs-fargate/index.md)

- Microservice with OpenTelemetry Collector
- Full Fargate Task & ECS configuration

### [AWS ECS EC2 (Python/Node.js)](otel/ecs-ec2/index.md)

- OpenTelemetry Collector container setup
- Sample app container & CloudFormation template

---

## Tracing Instrumentation Examples (Containers/Hosts)

### [Autogenerators](otel/autogenerators/index.md)

Demo apps with automatic tracing for:

- .NET
- Node.js
- Java
- Python

---

## Kubernetes-Specific Tracing

### [Autoinjection: Instrumentation Injection in Kubernetes](otel/autoinjection/index.md)

Auto-inject tracing for:

- .NET
- Node.js
- Java
- Python

### [Kubernetes eBPF: APM Without Instrumentation](otel/ebpf/index.md)

Use Linux Kernel’s eBPF to generate spans and dashboards **without modifying code or deployments**:

- .NET
- Node.js
- Java
- Python

---

## Manual Instrumentation

### [OpenTelemetry Manual Instrumentation](otel/manual-instrumentation/index.md)

Examples using the OpenTelemetry SDKs and APIs to send telemetry data (logs, metrics, traces) directly from applications.

---

## Real User Monitoring (RUM)

Measure frontend performance and correlate user sessions with backend telemetry:  
[RUM for Browser & Mobile](rum/index.md)

---

## Coralogix Live Demo Scripts

Install the OpenTelemetry Collector on a host or Kubernetes cluster:

- [Live Demo Tools Repo](https://github.com/coralogix/workshops/tree/master/livedemotools)

> **Be sure to modify variables appropriately and follow the official setup guides in** [Coralogix Docs](https://coralogix.com/docs/)

---

## Official OpenTelemetry Demo

Try out the official OpenTelemetry demo app:  
[OpenTelemetry Demo for Kubernetes](otel/opentelemetrydemo/index.md)
