
## Python Application Instrumentation
- This setup is tested in debian based (ubuntu) distribution.
- Enusure that you have python 3 running.
- Python app used in this example is emitting application logs to console.

### Set up Otel Contrib Collector on a Linux host.
- Download and install **OpenTelemetry Contrib** package for a linux distribution from [here](https://github.com/open-telemetry/opentelemetry-collector-releases/releases/)
- Extract a package that you downloaded using the correct package manager module, this will create and start **otelcol-contrib** service.
- otelcol-contrib uses a **config.yaml** file in the directory **/etc/otelcol-contrib/**
- Replace the **config.yaml** file with the example file in the [./otelcol](./otelcol) folder and update coralogix exporter section with your [api key](https://coralogix.com/docs/send-your-data-api-key/), [Coralogix domain](https://coralogix.com/docs/coralogix-domain/), and [application/subsystem names](https://coralogix.com/docs/application-and-subsystem-names/).
- If your application is writing logs to a file system, use [filelog receiver](https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/receiver/filelogreceiver) in the **config.yaml**.
- Restart the **otelcol-contrib** service.


### Run Python Client
This application make 100 requests to `https://reqbin.com/echo/get/json` and then exit.

Run following scripts found under [./python](.):
```
source setup-python.sh
source setup-python-env.sh
source start-python.sh
```

### Validate in Coralogix
#### Explore >> Logs UI
![Logs with Trace and Span Ids](images/LogsWithTraceId.png)
#### Explore >> Tracing UI
![Traces](images/Traces.png)
#### Individual Trace
![Trace Map](images/TraceMap.png)
#### Logs In-context
![Trace logs in-context](images/LogsInContext.png)