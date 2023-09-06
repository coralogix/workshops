FROM mcr.microsoft.com/dotnet/aspnet:6.0
# install OpenTelemetry .NET Automatic Instrumentation
ARG OTEL_VERSION=0.7.0
ADD https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation/releases/download/v${OTEL_VERSION}/otel-dotnet-auto-install.sh otel-dotnet-auto-install.sh
RUN apt -y update && apt install -y curl unzip
RUN export OTEL_DOTNET_AUTO_HOME="/otel-dotnet-auto" && sh otel-dotnet-auto-install.sh
RUN mkdir /App
COPY bin/Debug/net6.0/publish/ /App
COPY run-client.sh /App