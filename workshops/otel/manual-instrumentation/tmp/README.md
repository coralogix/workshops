# This repository is for OTEL instrumentation in Ruby
You can run this on your own EC2 host and in K8S 

## Run on Host
- Install ruby (3.2.3 was used for this repo)
- OtelCollector with config.yaml (running on the host)
- Be sure you have the gems for OpenTelemetry installed (see setupRuby.sh)
- You'll get a /bundle and a /vendor/bundle folder with the libraries installed

## Run on K8S (easier if you have cluster already set)
01-deploy-app.sh

## Remove 
02-delete-app.sh

## Telemetry Results  
A metric bucket is created called `operation_duration_seconds` and will be visible in a metrics explorer.    
The log message created is `Ruby logs coming through - operation took` with a number of seconds.  