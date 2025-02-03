# eBPF

## Requirements  
Prerequisites [here](https://coralogix.github.io/workshops/prereqs/)  
eBPF in this context only runs on Kubernetes with modern Linux kernel based containers.  

## Official Documentation [here](https://coralogix.com/docs/user-guides/apm/getting-started/ebpf-for-apm/)  
Please read the [official documentation](https://coralogix.com/docs/user-guides/apm/getting-started/ebpf-for-apm/) thoroughly before proceeding. There are many combinations possible with eBPF installation and this workshop only showcases one of them.  

## What is eBPF?  

[eBPF](https://ebpf.io/) describes software that runs inside the Linux kernel and can extend its functionality. For Observability purposes, the loaded module is capturing telemetry related to observability such as system calls, network activity, kernel function calls etc.  
  
The Coralogix implementation loads an eBPF agent into the kernels of running containers that exports the eBPF related telemetry and converts it to OpenTelemetry compatiable spans. eBPF spans do not provide traces since they don't have a trace ID. Please carefully read [official Coralogix Documenttion](https://coralogix.github.io/workshops/prereqs/)  above for more detail.


## Instructions  
 
This workshop is designed to add the eBPF agent to an existing installation of the Coralogix OpenTelemetry helm chart and then deploy simple applications in Python, Node, .NET, and Java to demonstrate the spans and APM capabilities available with eBPF.

### Step 1 - Install the OpenTelemetry Collector on your k8s cluster  
   
[Easy Coralogix instructions for Complete Observability are here](https://coralogix.com/docs/otel-collector-for-k8s/)  


### Step 2 - Clone Repo
```
git clone https://github.com/coralogix/workshops
```

### Step 3 - Change to eBPF Directory  
```
cd workshops/workshops/otel/ebpf
```  

### Step 4 - Override the OpenTelemetry Helm Chart to add the eBPF Agent
Edit `ebpf.yaml` and update the globals for your domain and cluster name then:  
```
source deploy-ebpf.sh
```  

### Step 5 - Deploy Example Applications
```
source deploy-all-examples.sh
```

### Step 6 - Study The Results  
Note in `Coralogix->Explore->Traces` that the requests are only one span deep, and that the TraceID is not truly utilized for connecting spans. Also note the `otel.library.name` of `coralogix-ebpf-agent`.  
  
  Coralogix->APM shows the working service catalog.  
    
  Other parts of the Kubernetes system are being traced as well- these can be filtered following instructions in the documentation above.

### Step 7 - Cleanup  
```
source delete-all-examples.sh
```  
```
helm rollback otel-coralogix-integration
```

