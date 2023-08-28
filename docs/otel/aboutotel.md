## OpenTelmetry

[OpenTelemetry](http://www.opentelemetry.io) is a standard organized by the [Cloud Native Compute Foundation](https://www.cncf.io/)comprised of two key elements:  
- A standard format for metrics, logs, and traces as well as standard protocols for shipping them
- Two pieces of software
    - The OpenTelemetry Collector which can run anywhere (host/container/local) and receives, processes, and exports telemetry
    - OpenTelemtry Tracing Instrumentation for many languages that instruments your apps to emit traces and works in two ways
        - Automatic Instrumentation: no code changes, instruments a long list of frameworks for each language
        - Manual Instrumentation that is added to your code for fine grain trace span building

## Why OpenTelemetry? Because Observability Should Use Universal Standards

Observability means measuring a system based on its outputs. In modern complex environment consisting of servers, containers, cloud, many application languages, and mixes and variations of all of the above, Observability is required to be able to proactively and/or in real time discover and repair problems however sytems are so diverse that traditional post-problem find/fix monitoring solutions no longer function or are scalable. Using Observability and monitoring systems and applications based on their outputs allows proper technology platform operations and problem prevention/remediation.  

Monitoring systems traditionally required negotiation with vendors to supply "monitoring software" and "agents" that are threaded througout an environment. This means annual negotiations are required to keep a business online because the "monitoring software" and "agents" are proprietary and technical debt of vendor relationships has to be cured on a regular basis. As systems grow larger and more distributed and complex, the private vendor lock on monitoring creates a fatal reliability flaw in the tech stack.  

No one negotiates for HTTP, SQL, HTTPS, TCP etc every year- so why should anyone negotiative for observability formats and agents? These should be open standard and included as part of day-to-day system building and operations.  

OpenTelemetry formats and tracing instrumentation allow for devops, operations, and development to scale infinitely without worry of vendor lock-in or future risk to an environment due to proprietary software.