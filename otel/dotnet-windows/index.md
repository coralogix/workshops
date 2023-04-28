# opentelemetry-dotnet-coralogix

## Description

OpenTelemetry on .net is comprised of only two major moving parts: the OpenTelemetry Collector and the OpenTelemetry .NET instrumentation. 

This is a guide on how to set up 
1. [OTEL Collector to send data to Coralogix](https://github.com/coralogix/opentelemetry-dotnet-coralogix#otel-collector-installation)
2. [.NET OpenTelemetry Agent on Windows](https://github.com/coralogix/opentelemetry-dotnet-coralogix#net-instrumentation)
3. [Example application for testing.](https://github.com/coralogix/opentelemetry-dotnet-coralogix#example-application)

## OTEL Collector Installation
You can download the collector from the [OpenTelemetry Collector releases page](https://github.com/open-telemetry/opentelemetry-collector/releases). Download the file for your platform (in this case, Windows) and version (e.g. v0.75.0). 

You can use this [config](https://github.com/coralogix/OpenTelemetry-.NET/blob/master/Example/Collector/config.yaml) as a template for you OTEL collector config.
In the config.yaml, adjust the domains and your Coralogix private key shown in the snippet below. Also define your application & subsystem name tags.

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

## .NET Instrumentation

You can find the complete instruction for .NET OpenTelemetry Agent Instrumentation is in [.NET OpenTelemetry GitHub Repository](https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation). 

### Quick start

Please run the commands below in your PowerShell. Please update the `"MyServiceDisplayName"`
```powershell
# Download the module
$module_url = "https://raw.githubusercontent.com/open-telemetry/opentelemetry-dotnet-instrumentation/v0.7.0/OpenTelemetry.DotNet.Auto.psm1"
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

<small>Note: You'll see the tracing data in Coralogix if you have a OTEL collector forwarding data received at the specified endpoint to Coralogix. Please follow the instructions below for configuring the OTEL Collector for Coralogix.</small>

## Example Application

You can download & instrument the [example application](https://github.com/coralogix/OpenTelemetry-.NET/tree/master/Example/dotnet) provided in this repository for testing. The example sends traces of `http get` requests of a public URL.
