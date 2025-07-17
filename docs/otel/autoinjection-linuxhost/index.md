# Autoinjection  

## Requirements  
Prerequisites [here](https://coralogix.github.io/workshops/prereqs/)  

## What Is Autoinjecton?  

OpenTelemetry can inject, via installer, zero code tracing instrumentation into deployments that use Java, Node, .NET.  

This is a great way to easily deploy tracing at scale without changing any code.

Official documentation is here: [https://github.com/open-telemetry/opentelemetry-injector](https://github.com/open-telemetry/opentelemetry-injector)  

At the time of this writing 6/2025, there are no builds of the deb/rpm installers on Github- we are supplying them until there are.  
Build instructions are also available in the Github README.md if one wants to build their own deb/rpm installer.  

### Step 1 - Clone Repo
```
git clone https://github.com/coralogix/workshops
```

### Step 2 - Change to Autoinject Directory
```
cd workshops/workshops/otel/autoinject-linuxhost
```

### Step 3 - Install Injector  

This script should detect deb/rpm installer appropriate for your Linux and install it along with per language instrumentation:  
```
source install-otel-injector.sh
```  
### Step 4 - Deploy Examples
  
Single command deploys examples in Java, .NET 8, Node are in each language subdirectory to deploy:
    
`dotnet8-linux`  
```
dotnet run
```  
  
`java`
```
source 02-run-app.sh
```
  
`node` 
```
node app.js
```
  
Study the traces in Coralogix.  
  
Cleanup:  
```
sudo ./03-cleanup-uninstall.sh  
```
