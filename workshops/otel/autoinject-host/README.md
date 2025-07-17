# OpenTelemetry Auto-Inject Host Setup

Scripts for building, installing, and testing OpenTelemetry auto-instrumentation on Linux hosts.

## How It Works

OpenTelemetry auto-injection uses `LD_PRELOAD` to automatically instrument applications without code changes. A shared library intercepts system calls and adds telemetry to applications written in Java, .NET, Node.js, Python, etc.

## Scripts

### Installing OpenTelemetry Instrumentation  
- `install-otel-injector.sh` - Install appropriate package (.deb or .rpm) based on system type
  - Auto-detects DEB vs RPM systems
  - Offers system-wide or systemd-only injection
  - Creates configuration files in `/etc/opentelemetry/otelinject/`

### Sample Applications (for testing)
- `java/` - Spring Boot application with Maven
- `dotnet8-linux/` - .NET 8 web application  
- `node/` - Node.js HTTP server

### Building OpenTelemetry Instrumentation (Optional for those who wish to build)
Run as root:
- `00-install-docker-debian.sh` - Install Docker (optional, for containerized apps)
- `01-setup-build-env.sh` - Install build dependencies (git, build-essential, etc.)
- `02-build-otel-injector.sh` - Build both .deb and .rpm packages from source
- `03-cleanup-uninstall.sh` - Remove installations but preserve built packages

## Requirements

- Ubuntu/Debian or RHEL/CentOS/Fedora
- Root privileges (sudo)
- Internet connectivity
- 2GB RAM, 1GB disk space

## Configuration

Set OpenTelemetry collector endpoint:
```bash
export OTEL_EXPORTER_OTLP_ENDPOINT=http://your-collector:4317
export OTEL_SERVICE_NAME=your-app-name
```

## Cleanup

```bash
# Remove installation but keep built packages
sudo ./03-cleanup-uninstall.sh
```