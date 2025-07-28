# eBPF for Kubernetes

## Requirements  
Prerequisites [here](https://coralogix.github.io/workshops/prereqs/)  
eBPF in this context only runs on Kubernetes with modern Linux kernel based containers.  
Note that eBPF integration is currently in a beta state and is subject to change.  

## Official Documentation [here](https://coralogix.com/docs/user-guides/apm/getting-started/ebpf-for-apm/)  
Please read the [official documentation](https://coralogix.com/docs/user-guides/apm/getting-started/ebpf-for-apm/) thoroughly before proceeding. There are many combinations possible with eBPF installation and this workshop only showcases one of them.  

## What is eBPF?  

[eBPF](https://ebpf.io/) describes software that runs inside the Linux kernel and can extend its functionality. For Observability purposes, the loaded module is capturing telemetry related to observability such as system calls, network activity, kernel function calls etc.  
  
The Coralogix implementation loads an eBPF agent into the kernels of running containers that exports the eBPF related telemetry and converts it to OpenTelemetry compatiable spans. eBPF spans do not provide traces since they don't have a trace ID. Please carefully read [official Coralogix Documenttion](https://coralogix.github.io/workshops/prereqs/)  above for more detail.


## Instructions  
 
This workshop is designed to add the eBPF agent to an existing installation of the Coralogix OpenTelemetry helm chart and then deploy simple applications in Python, Node, .NET, and Java to demonstrate the spans and APM capabilities available with eBPF.

### Install the OpenTelemetry Collector on your k8s cluster
   
[Easy Coralogix instructions for Complete Observability are here](https://coralogix.com/docs/otel-collector-for-k8s/)  

### Clone Repo
```
git clone https://github.com/coralogix/workshops
```

### Change to eBPF Directory  
```
cd workshops/workshops/otel/ebpf
```  

### Step 1 - Deploy the OpenTelemetry Helm Chart with eBPF Instrumentation
Edit `ebpf.yaml` and update the globals for your domain and cluster name then:  
```
source 01-helm-otel-ebpf.sh
```  

### Step 2 - Deploy Example Applications  
We will reuse the zero-instrumentaiton examples from the `autoinjection` workshop.  
These are applications that have no OpenTelemetry instrumentaion in their containers.  
```
cd workshops/workshops/otel/autoinject
source deploy-all-examples.sh
```

### Study The Results  
Start with `Coralogix->Explore->Traces` to examine the spans.
  
  Coralogix->APM shows the working service catalog.  
    
  Other parts of the Kubernetes system are being traced as well- these can be filtered following instructions in the documentation above.

### Step 3 - Cleanup  

Delete the app examples:  
```
source delete-all-examples.sh
```  
Return to the eBPF workshop and delete the otel collector
```
cd workshops/workshops/otel/ebpf
source 04-delete-cx-otel.sh
```