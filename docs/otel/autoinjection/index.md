# Autogenerators

## Requirements  
- Prerequisites [here](https://coralogix.github.io/workshops/prereqs/)  

## What Is An Autoinjecton?  

OpenTelemetry has an operator that can inject zero code tracing instrumentation into deployments that use Java, Nodejs, Go, .NET, and Python.  

This is a great way to easily deploy tracing at scale without changing any code. Further instrumentation configuration can be done via each app's `deployment.yaml` if desired but this is not necessary.  

Official documentation is here: [https://opentelemetry.io/docs/kubernetes/operator/automatic/](https://opentelemetry.io/docs/kubernetes/operator/automatic/)  

The autoinjection examples in this workshop all have Dockerfiles, deploy/delete scripts, app code, and everything else needed to easily try this capability.  

Note that if a k8s cluster is configured with autoinjection applied to running deployments, they must be restarted with their annotations for the tracing to begin.  

### Step 1 - Clone Repo
```
git clone https://github.com/coralogix/workshops
```

### Step 2 - Change to Autoinject Directory
```
cd workshops/workshops/otel/autoinject
```

### Step 3 - Prep Kubernetes Cluster

Cluster prep after Coralogix k8s integration installed:  
- Install cert manager to enable https endpoints within the cluster  
- Install the OpenTelemetry operator  

```
helm repo add jetstack https://charts.jetstack.io --force-update

helm install cert-manager jetstack/cert-manager   --namespace cert-manager   --create-namespace   --version v1.15.3   --set crds.enabled=true

helm repo add open-telemetry https://open-telemetry.github.io/opentelemetry-helm-charts

helm install opentelemetry-operator open-telemetry/opentelemetry-operator --set "manager.collectorImage.repository=otel/opentelemetry-collector-k8s"
```   

Next apply OpenTelemetry Operator instrumentation:  
```
source apply-instrumentation.sh
```  
### Step 4 - Deploy Examples
  
Single command deploys examples in Java, .NET 8, Node, and Python  
```
source deploy-all-examples.sh
```  

Study the traces in Coralogix.  


Cleanup:  
```
source delete-all-examples.sh
source delete-instrumentation.sh
```

### Step 5 - Study Examples
  
Each directory has a `deployment.yaml` and a script to deploy and delete it.    

App deployments must have an *annotation* to ensure that they will have instrumentation injected i.e.  
```
  template:
    metadata:
      labels:
        name: cx-autoinject-java
      annotations:
        instrumentation.opentelemetry.io/inject-java: "true"
```