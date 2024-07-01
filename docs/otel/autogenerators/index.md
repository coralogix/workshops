# Autogenerators

## Requirements  
- [These prerequisites](https://coralogix.github.io/workshops/prereqs/)  

## About

An `autogenerator` is a reference implementation of OpenTelemetry that can run, alone, as a application and emits interesting telemetry (metrics/traces/logs) to use as a 'how-to' template for your own OpenTelemetry use.  

The goal of a reference implementation is to answer the question "how do I instrument my app for OpenTelemetry?" Via showing a simple live example app that you can try yourself and then refer to for your own projects.  


## Instructions  
 
Each autogenerator project contains the following:

- Application source code
- `buildcontainer.sh` script for building a container  
- `Dockerfile` to demonstrate containerization
- .yaml deployment example for Kubernetes
- Various scripts useful in building, deploying, and running the app (these vary)
- Scripts in order to accomplish a goal i.e. `1-deploy-for-k8s.sh` and `2-delete-from-k8s.sh`  
- Notes about each one ***on this page***. There are no README.md instructions in each repo.

Although the container examples are for Kubernetes, you can port them to any container style environment so long as required ports are open and there are no prohibitions for the deployment making requests of itself.  

Most of these example create an `http client` and `http server` in a loop so that client and server spans are generated.  

These examples are under frequent revision so please open an issue with any bug reports.  

***Each script needs to be checked for variables that need to be changed in your environment***

## The Autogenerators

### Step 1 - Clone Repo
```
git clone https://github.com/coralogix/workshops
```

### Step 2 - Change to Autogenerator Directory
```
cd workshops/workshops/otel/autogenerators
```

`dotnet6-linux`
- .NET 6 app on Linux container  
- Generates looping requests of `api.github.com`
- Makes error spans

`dotnet8-linux`
- .NET 8 app on Linux container  
- Generates looping requests of `api.github.com`
- Makes error spans

`java`
- Java examples for container and monolith built with Maven   
- Generates looping requests of a self running Spring server  
- Trace and span ID log auto-injection using `log4j2`  
- Requires Coralogix parsing `Body` field that contains Trace and Span IDs