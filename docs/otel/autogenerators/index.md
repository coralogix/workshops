# Autogenerators

## Requirements  
Prerequisites [here](https://coralogix.github.io/workshops/prereqs/)  

## What Is An Autogenerator?  

An `autogenerator` is a reference implementation of OpenTelemetry that can run as a ***standalone*** single deployment application and emits interesting telemetry (metricsm, traces, and/orlogs) to use as a 'how-to' template for your own OpenTelemetry use.  

The goal of a reference implementation is to answer the question "how do I instrument my app for OpenTelemetry?" via showing a simple live example app that you can try yourself and then refer to for your own projects.  
  
## Instructions  
 
Each autogenerator project contains the following:

**All Examples**  
- Application source code  
- Various scripts useful in building, deploying, and running the app. These vary but are labelled clearly i.e. `deploy.sh` and `delete.sh`.  
- Scripts sometimes in order to accomplish a goal i.e. `1-configure.sh` and `2-build.sh`  
- Each script needs to be checked for variables that need to be changed in your environment  
- Notes about each one ***on this page***. There are no README.md instructions in each repo.  
  
**Container Examples**  
- `buildcontainer.sh` script for building a container  
- `Dockerfile` to demonstrate containerization  
- .yaml deployment example for Kubernetes  
- Although the container examples are for Kubernetes, you can port them to any container style environment so long as required ports are open and there are no prohibitions for the deployment making requests of itself.  
  
**Host Examples**  
- Env variable and other setup/run scripts  
- Otel collector `config.yaml` if needed  
- Host logs are NOT shipped by Otel Collector- if you want to ship them, they can be written to `/var/` with an updated Otel collector config  
  
Most of these example create an `http client` and `http server` in a loop so that client and server spans are generated.  

These examples are under frequent revision so please open an issue with any bug reports.  

Before beginning, study [Primary Otel docs for instrumentation](https://opentelemetry.io/docs/zero-code/)  

## The Autogenerators  

### Step 1 - Clone Repo
```
git clone https://github.com/coralogix/workshops
```

### Step 2 - Change to Autogenerator Directory
```
cd workshops/workshops/otel/autogenerators
```

`dotnet8-linux`  
- .NET 8 app on a Linux container  
- Generates looping requests of dual client/server .NET app
- Logs with `Microsoft.Extensions.Logging`  
- [Otel Docs/Repo](https://opentelemetry.io/docs/zero-code/dotnet/)  
  
`dotnet6-linux`  
- .NET 6 app on a Linux container  
- Generates looping requests of `api.github.com`  
- Makes error spans  
- [Otel Docs/Repo](https://opentelemetry.io/docs/zero-code/dotnet/)  
  
`go`
- Container / host examples
- Go app with self requesting client/server app
- Trace/span logging
- [Otel Docs/Repo](https://github.com/open-telemetry/opentelemetry-go-instrumentation/)  
  
`java`
- Container / host built with Maven   
- Generates looping requests of a self running Spring server  
- Trace and span ID log auto-injection using `log4j2`  
- Requires Coralogix parsing `Body` field that contains Trace and Span IDs  
- [Otel Docs/Repo](https://opentelemetry.io/docs/zero-code/java/)  

`node`
- Container / host examples  
- Generates looping requests of a self running Express server  
- Trace and span ID log injection using `Pino`  
- Does not require parsing for Trace and span IDs  
- [Otel Docs/Repo](https://opentelemetry.io/docs/zero-code/js/)  

`php`
- Container examples  
- Generates looping requests of a self running php server and also includes a laraavel example  
- [Otel Docs/Repo](https://opentelemetry.io/docs/zero-code/php/)  
  
`python`
- Container / host examples  
- Generates looping requests of a self running Flask server  
- Trace and span ID log injection using `Python Logger`  
- Does not require parsing for Trace and span IDs  
- [Otel Docs/Repo](https://opentelemetry.io/docs/zero-code/python/)  