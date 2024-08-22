**Autoinjection WIP- updated 2024-08-21**  

This is work in process!  
Each language example contains a non instrumented tracing app autogenerator:  
`dotnet8`  
`java`  
`node`  
`python`  

**Instructions:**    
```
git clone https://github.com/coralogix/workshops
cd ./workshops/workshops/otel/autoinject/
```
  
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
One applied, start all the example services- node, python, java, dotnet8:  
```
source deploy-all-examples.sh
```

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

2024-08-21: Java, Python, .NET, node tested and working