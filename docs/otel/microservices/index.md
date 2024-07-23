# OpenTelemetry Microservices / Kubernetes

## Instructions

Requirements:  
- [These prerequisites](https://coralogix.github.io/workshops/prereqs/)  
- A Kubernetes cluster i.e. EKS or GKE for a sandbox environment. If you cannot procure EKS or GKE **See Appendix at the end of this page for a Kubernetes sandbox environment setup.**

### Step 1 - Install the OpenTelemetry Collector on your k8s cluster  
   
[Easy Coralogix instructions for Complete Observability are here](https://coralogix.com/docs/otel-collector-for-k8s/)  

### Step 2 - Clone workshop
```
git clone https://github.com/coralogix/workshops
```

### Step 3 - Change to workshop dir
```
cd workshops/workshops/otel/microservices
```

### Step 4 - Deploy example to k8s
If you want to change the namespace, edit `yaml/deploy-all.yaml`  
Tthree services will appear:  

- `cx-shopping-cart-reqs` - a requesting service initiating a transaction  
- `cx-payment-gateway-flask` - a server that is a bridge for a transaction to a database- returns a transaction ID to the `shopping-cart`  
- `cx-redis` - an instance of a redis database used for a transaction

```
source deploy-all.sh
```

Deploys the following as seen from the `http://cx-payment-gateway-flask:5000/` root span:  
  
Note: images may vary from actual UI due to frequent updates:  
<img src="https://coralogix.github.io/workshops/images/microservices-workshop/01.png" width=540>     
<!-- ![Microservices Workshop](../../images/microservices-workshop/01.png) -->
With healthy low latency spans for all services:  
  
<img src="https://coralogix.github.io/workshops/images/microservices-workshop/03.png" width=540>  

### Step 5 - Simulate CI/CD scenarios
Study results in Coralogix portal

Simulate a "bad" deployment:  
```
source deploy-bad.sh
```

This deployment will cause severe sporadic problems in `payment-gateway` such as 500s and latency in the service response along with a drop in transaction volume. You can see the latency spikes here:    

<img src="https://coralogix.github.io/workshops/images/microservices-workshop/04.png" width=540>   

Alerts and automation can be built around span latency or Payment Gateway 500 responses.  

Roll back the bad deployment for the services to return to normal:  
```
source deploy-good.sh
```  

Span latency will return to normal and Payment Gateway 500 responses will cease.  

### Step 6 - Study how OpenTelemetry tracing instrumentation works  
   
**OpenTelemetry Instrumentation For Containerized Apps**  
- Dockerfiles for the containers contain OpenTelemetry auto instrumentation for Python and are in `/python` root level  
- Study Otel Python [Auto Instrumentation](https://opentelemetry.io/docs/instrumentation/python/automatic/)
- Notice how the [Dockerfile](https://github.com/coralogix/workshops/blob/master/workshops/otel/microservices-demo/python/dockerfile-microsvcsdemo) adds the automatic instrumentation  
- And observe how the [kickstart script](https://github.com/coralogix/workshops/blob/master/workshops/otel/microservices-demo/python/k8s/start-reqs-envurl-otel.sh) uses the instrumenting command  

**Kubernetes Deployments for Otel Tracing Instrumentation**    
- `.yaml` deployment files are in `python/yaml` and show how environment variables are used to control the instrumentation
- study the [deploy-all.yaml]((https://github.com/coralogix/workshops/tree/master/workshops/otel/microservices/yaml)) which shows the environment variables that control OpenTelemetry tracing instrumentation. Focus on how the [Kubernetes Downward API](https://kubernetes.io/docs/concepts/workloads/pods/downward-api/) use of `status.hostIP` to tell the instrumentation where to send traces: to the IP adddress of the host node on port 4317 - this is the default GRPC endpoint for OTLP trace spans    

**Instrumented Applications and Frameworks**  
- The Python apps that drive this example are in the `python/apps` dir  
- Examine the frameworks used to show how tracing picks up their execution   
- Note that the `python requests` library is used to make http requests, and that [OpenTelemetry Python Instrumentation](https://opentelemetry.io/docs/instrumentation/python/automatic/) lists `requests` as an [automatically instrumented library](https://opentelemetry.io/ecosystem/registry/?language=python&component=instrumentation)  
    
## Cleanup
To remove all the deployments/services/pods from the example from your k8s cluster (ignore any errors it reports):  
```
source delete-all.sh
```
  
## Appendix - Quick Kubernetes Sandbox Setup  
- AWS EC2 t2.2xl VM with 24GB of storage w/ Ubuntu Linux or equivalent other cloud provider  
- A quick k8s platform is k3s minimal Dockerless Kubernetes- use script top right of homepage: [https://k3s.io/](https://k3s.io/) 
- Every time you open a new shell, to use k3s for helm, kubectl, etc you must set the appropriate env variable:  
    ```
    sudo chmod 644 /etc/rancher/k3s/k3s.yaml && export KUBECONFIG=/etc/rancher/k3s/k3s.yaml  
    ```  
    This script is included in the workshop in the k3s folder: `source ./k3s/k3s.sh`  
- Install helm- use bash script: [https://helm.sh/docs/intro/install/](https://helm.sh/docs/intro/install/)  
- Install k9s kubernetes monitor: [https://k9scli.io/](https://k9scli.io/) 
    - Download appropriate release and gunzip / tar -xf the release [https://github.com/derailed/k9s/releases](https://github.com/derailed/k9s/releases)