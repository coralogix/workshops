## OpenTelemetry for .NET / Coralogix

### Instructions

This example is for basic study only and is not documentation.    
Full documentation: [https://coralogix.com/docs/](https://coralogix.com/docs/)  
Requirements:  
- Windows Server 2019 or newer    
- Updated versions and sufficient permissions for downloading and installing software and no restrictions on GitHub domain    
- .NET Framework 3.5 or greater installed   
- PowerShell 5.1 or higher  
- Proper IDE i.e. Visual Studio Code 

### About OpenTelemetry For .NET  
  
You can find the complete instruction for .NET OpenTelemetry Agent Instrumentation is in [.NET OpenTelemetry GitHub Repository](https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation). 

There are three key steps to sending traces from a .NET application on Windows:  
- Install the OpenTelemetry collector  
- Install OpenTelemetry .NET instrumentation  
- Run a sample .NET application to test traces  
  
### Step 1 - Setup  
Clone repo:
```
git clone https://github.com/coralogix/workshops
```  
  
### Step 2 - OTEL Collector Installation    
Download and install latest CONTRIB release version from here:  
[https://github.com/open-telemetry/opentelemetry-collector-releases/releases](https://github.com/open-telemetry/opentelemetry-collector-releases/releases)  

Use the following example from this repo a template for the OTEL collector config:  
`~/workshops/workshops/otel/dotnet-windows/Collector/config.yaml`  

In `config.yaml` adjust the domains and your Coralogix private key shown in the snippet below. Also define your application & subsystem name tags.

References:  
- [Coralogix Endpoints](https://coralogix.com/docs/coralogix-endpoints/)  
- [Coralogix Private Key](https://coralogix.com/docs/private-key/)  
- [Application & Subsystem Names](https://coralogix.com/docs/application-and-subsystem-names/)


```yaml
#config.yaml

exporters:
  logging:
    verbosity: detailed
  coralogix:
    # The Coralogix traces ingress endpoint
    traces:
      endpoint: "YOURDOMAINHERE i.e. ingress.coralogix.us:443"
    metrics:
      endpoint: "YOURDOMAINHERE i.e. ingress.coralogix.us:443"
    logs:
      endpoint: "YOURDOMAINHERE i.e. ingress.coralogix.us:443"

    # Your Coralogix private key is sensitive
    private_key: "YOURKEYHERE"

...

    # Traces, Metrics and Logs emitted by this OpenTelemetry exporter 
    # are tagged in Coralogix with the default application and subsystem constants.
    application_name: "MyBusinessEnvironment"
    subsystem_name: "MyBusinessSystem"
```

Next: run the collector using the command

```powershell
otelcol-contrib.exe --config=config.yaml
```

### Step 3 - Install .NET Instrumentation

You can find the complete instructions for .NET OpenTelemetry Agent Instrumentation is in [.NET OpenTelemetry GitHub Repository](https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation).  

Below is a shortcut set of instructions but these may be updated in the official documentation above, so we recommend following the officical set.  

#### Quick start

The official Otel instrumentation guide shows the following:  
Run the commands below in your PowerShell. Update the `"MyServiceDisplayName"` with the name of your application.  

```powershell
# Download the module
$module_url = "https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation/releases/latest/download/OpenTelemetry.DotNet.Auto.psm1"
$download_path = Join-Path $env:temp "OpenTelemetry.DotNet.Auto.psm1"
Invoke-WebRequest -Uri $module_url -OutFile $download_path -UseBasicParsing

# Import the module to use its functions
Import-Module $download_path

# Install core files
Install-OpenTelemetryCore

# Set up the instrumentation for the current PowerShell session
Register-OpenTelemetryForCurrentSession -OTelServiceName "MyServiceDisplayName"
```

Next, configure the follwing Environment Variables

```powershell
$env:OTEL_RESOURCE_ATTRIBUTES='service.name=dotnetsvc,application.name=dotnetapp cx.application.name=dotnetappcx,cx.subsystem.name=dotnetsubcx'
$env:OTEL_EXPORTER_OTLP_TRACES_ENDPOINT='localhost:4317'
$env:OTEL_EXPORTER_OTLP_TRACES_PROTOCOL='grpc'
```

Finally, run your dotnet application with

```powershell
dotnet run
```

### Step 4 - Example Application

You can download & instrument the example application:  
`~/workshops/workshops/otel/dotnet-windows/ExampleApp`  
provided in this repository for testing.  

The example sends traces of `http get` requests of a public URL.   
Simply use `dotnet run` to start the application.  

You may want to open it first in Visual Studio 2022 or higher to ensure dependencies are installed first and then run from the Visual Studio 2022 console.

### ASP.NET

IIS / ASP.NET follows a different instruction path for instrumentation and is shown here:  
[https://opentelemetry.io/docs/instrumentation/net/automatic/](https://opentelemetry.io/docs/instrumentation/net/automatic/)