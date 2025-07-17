# OpenTelemetry Auto-Inject Host Setup

This directory contains a set of Bash scripts to automate the setup, installation, and uninstallation of the OpenTelemetry Auto-Injector on a host system. These scripts are designed for Ubuntu/Debian-based distributions and provide a comprehensive solution for automatic instrumentation of applications.

## How OpenTelemetry Auto-Injection Works

The OpenTelemetry Auto-Injector uses a system-level mechanism called `ld.so.preload` to automatically instrument applications without requiring code changes. Here's how it works:

1. **Dynamic Library Preloading**: The system loads a special OpenTelemetry library (`libotelinject.so`) before any application starts
2. **Function Interception**: This library intercepts key system calls and function calls that applications make
3. **Automatic Instrumentation**: When applications make network requests, database calls, or other instrumented operations, the library automatically captures telemetry data
4. **Zero Code Changes**: Applications run normally but now emit OpenTelemetry traces, metrics, and logs automatically

This approach works with applications written in various languages (Java, .NET, Node.js, Python, etc.) without requiring any modifications to the application code.

## Scripts Overview

The installation process is split into focused, modular scripts:

### Core Environment Setup
*   `00-setup-env.sh`: Sets up the minimal necessary environment including build tools (build-essential), git, and basic utilities required for building the OpenTelemetry injector.

### Optional Docker Installation  
*   `00-install-docker-debian.sh`: Comprehensive Docker Engine installation script for Debian-based systems. This is optional and only needed if you plan to use Docker-based workshops or containerized applications.

### OpenTelemetry Auto-Injector Installation
*   `02-build-otel-injector.sh`: Downloads the OpenTelemetry Injector source code, compiles it, and creates a Debian package. This step builds the actual injection library from source.
*   `03-install-otel-injector.sh`: Installs the built package and configures the system for auto-injection. This includes setting up `/etc/ld.so.preload` and systemd configurations.

### Cleanup
*   `04-uninstall-autoinject-host.sh`: Safely removes the OpenTelemetry Injector package and cleans up all related configurations.

## Prerequisites

*   An Ubuntu/Debian-based operating system
*   Root privileges (sudo access)
*   Internet connectivity to download packages and clone repositories
*   Minimum 2GB RAM and 1GB free disk space for building the injector

## Usage

Follow these steps to set up your environment and install the OpenTelemetry Auto-Injector:

### Step 1: Set up the Build Environment

This script installs the essential build tools needed to compile the OpenTelemetry injector:

```bash
sudo ./00-setup-env.sh
```

**What this installs:**
- `build-essential`: GCC compiler, make, and other build tools
- `git`: For cloning the OpenTelemetry injector repository  
- `curl` and `ca-certificates`: For secure downloads during the build process

### Step 2: Optional - Install Docker (if needed)

If you plan to use Docker-based applications or other workshops that require Docker, install it separately:

```bash
sudo ./00-install-docker-debian.sh
```

**Important:** After running this script, you must **log out and log back in** for the Docker group changes to apply. You can then verify your Docker installation by running `docker run hello-world`.

### Step 3: Build the OpenTelemetry Injector

This script downloads the source code and builds the OpenTelemetry injector package:

```bash
sudo ./02-build-otel-injector.sh
```

**What this does:**
- Clones the official OpenTelemetry injector repository
- Installs build dependencies (cmake, libssl-dev, etc.)
- Compiles the injector library and creates a Debian package
- Validates the build and cleans up temporary files

### Step 4: Install and Configure the Auto-Injector

This script installs the built package and configures your system for automatic instrumentation:

```bash
sudo ./03-install-otel-injector.sh
```

**What this does:**
- Installs the OpenTelemetry injector Debian package
- Configures `/etc/ld.so.preload` for system-wide injection (optional)
- Sets up systemd-only injection as an alternative approach
- Creates configuration files in `/etc/opentelemetry/otelinject`
- Validates the installation

**Configuration Options:**
The script offers two injection methods:
1. **System-wide injection**: Uses `/etc/ld.so.preload` to inject into all processes
2. **Systemd-only injection**: Only injects into systemd services (safer approach)

You can modify the configuration files in `/etc/opentelemetry/otelinject` to:
- Set your OpenTelemetry collector endpoint
- Configure service names and resource attributes
- Enable/disable specific instrumentation libraries
- Set sampling rates and other telemetry options

### Step 5: Install and Run Sample Applications

This directory includes three sample applications to demonstrate the OpenTelemetry Auto-Injector in action. Each application is located in its own subdirectory and can be installed and run independently.

**Important**: Make sure you have completed Steps 1-4 above before running these sample applications, as they depend on the auto-injector being properly installed and configured.

#### Java Application

The Java application is a Spring Boot application that demonstrates automatic instrumentation.

**Installation:**
```bash
cd java
sudo ./00-install-java-maven.sh
./01-build-app.sh
sudo ./02-run-app.sh
```

**What this does:**
*   Installs Java 8 and Apache Maven 3.9.6
*   Builds the Spring Boot application using Maven
*   Creates a systemd service to run the application as a background service

**Service Management:**
```bash
# Check service status
sudo systemctl status java-app.service

# View logs
sudo journalctl -u java-app.service -f

# Stop the service
sudo systemctl stop java-app.service

# Start the service
sudo systemctl start java-app.service
```

#### .NET 8 Application

The .NET application is a web application built with .NET 8.

**Installation:**
```bash
cd dotnet8-linux

# Install .NET 8 SDK (if not already installed)
wget -O - https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /usr/share/keyrings/microsoft-archive-keyring.gpg
echo "deb [arch=amd64,arm64,armhf signed-by=/usr/share/keyrings/microsoft-archive-keyring.gpg] https://packages.microsoft.com/repos/microsoft-ubuntu-$(lsb_release -cs)-prod $(lsb_release -cs) main" | sudo tee /etc/apt/sources.list.d/microsoft-prod.list
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0

# Build and run the application
dotnet build
dotnet run
```

**Alternative: Run as a service**
```bash
# Build the application
dotnet publish -c Release -o /opt/dotnet-app

# Create a systemd service (manual setup required)
sudo systemctl edit --full dotnet-app.service
```

#### Node.js Application

The Node.js application is a simple HTTP server that demonstrates automatic instrumentation.

**Installation:**
```bash
cd node

# Install Node.js and npm (if not already installed)
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt-get install -y nodejs

# Install application dependencies
npm install http uuid pino

# Run the application
node app.js
```

**Alternative: Run as a service**
```bash
# Install PM2 for process management
sudo npm install -g pm2

# Start the application with PM2
pm2 start app.js --name "node-app"

# Save PM2 configuration
pm2 save
pm2 startup
```

### Step 6: Uninstall OpenTelemetry Auto-Injector

When you're done with the workshop or want to clean up your system, use this script to safely remove the OpenTelemetry Injector:

```bash
sudo ./02-uninstall-autoinject-host.sh
```

**What this does:**
- Removes the OpenTelemetry Injector Debian package
- Safely removes only the OpenTelemetry-specific entries from `/etc/ld.so.preload` 
- Cleans up configuration files in `/etc/opentelemetry/otelinject`
- Removes build artifacts and temporary files
- Preserves other system components (Docker, build tools remain installed)

**Important:** This script carefully removes only the OpenTelemetry Injector-specific line from `/etc/ld.so.preload` to avoid affecting other system components. It does **not** uninstall Docker or `build-essential` installed by the setup script, as those are considered broader system dependencies.

## Verifying the Installation

After completing the installation, you can verify that the OpenTelemetry Auto-Injector is working correctly:

### 1. Check Installation Status
```bash
# Verify the package is installed
dpkg -l | grep opentelemetry

# Check if the library is loaded
cat /etc/ld.so.preload | grep libotelinject

# Verify configuration files exist
ls -la /etc/opentelemetry/otelinject/
```

### 2. Test with a Simple Application
```bash
# Run a simple command and check if it's instrumented
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317 \
OTEL_SERVICE_NAME=test-app \
curl -I http://example.com
```

### 3. Monitor Telemetry Data
If you have an OpenTelemetry collector running, you should see traces and metrics being generated automatically by applications.

## Configuration Details

The OpenTelemetry Auto-Injector can be configured through environment variables and configuration files:

### Environment Variables
- `OTEL_EXPORTER_OTLP_ENDPOINT`: Your OpenTelemetry collector endpoint
- `OTEL_SERVICE_NAME`: Name of the service being instrumented
- `OTEL_RESOURCE_ATTRIBUTES`: Additional resource attributes
- `OTEL_TRACES_SAMPLER`: Sampling strategy (always_on, always_off, traceidratio)

### Configuration Files
- `/etc/opentelemetry/otelinject/config.yaml`: Main configuration file
- `/etc/opentelemetry/otelinject/instrumentation.conf`: Instrumentation library settings
- `/etc/systemd/system/opentelemetry-injector.service`: Systemd service configuration

## Security Considerations

The OpenTelemetry Auto-Injector operates at a system level and has the following security implications:

1. **System-wide Impact**: When using `/etc/ld.so.preload`, the injector affects all processes on the system
2. **Root Privileges Required**: Installation requires root access due to system-level modifications
3. **Systemd-only Mode**: Consider using systemd-only injection for production environments to limit scope
4. **Network Communication**: The injector will make network calls to your OpenTelemetry collector
5. **Process Monitoring**: The injector intercepts system calls, which may be flagged by security tools

## Application Verification

After installing the applications, you can verify that the OpenTelemetry Auto-Injector is working by:

1. **Checking application logs** for OpenTelemetry traces and initialization messages
2. **Monitoring the applications** using your configured OpenTelemetry backend (Jaeger, Zipkin, etc.)
3. **Running the verification script** (if available):
   ```bash
   cd java
   ./verify-autoinject.sh
   ```
4. **Using OpenTelemetry debugging tools**:
   ```bash
   # Enable debug logging
   export OTEL_LOG_LEVEL=debug
   
   # Run your application and check for telemetry output
   your-application
   ```

## Troubleshooting

### Common Issues

**"This script must be run as root"**
- Ensure you are running the installation scripts with `sudo`

**Docker commands not working after setup**
- Remember to log out and log back in after running `00-install-docker-debian.sh` to apply the `docker` group changes

**Build failures during compilation**
- Ensure you have sufficient disk space (minimum 1GB free)
- Check that all build dependencies are properly installed
- Verify internet connectivity for downloading source code

**Auto-injection not working**
- Verify the library is listed in `/etc/ld.so.preload`
- Check that environment variables are properly set
- Ensure your application is using supported libraries/frameworks
- Try running with debug logging enabled

**Performance issues**
- Consider switching from system-wide to systemd-only injection
- Adjust sampling rates in your configuration
- Monitor resource usage and adjust accordingly

**Application-specific issues**
- Check the individual application directories for specific troubleshooting information
- Verify that your application's language/framework is supported
- Review application logs for OpenTelemetry-related errors

### Getting Help

If you encounter issues not covered here:
1. Check the OpenTelemetry Injector documentation
2. Review system logs: `journalctl -xe`
3. Enable debug logging and examine the output
4. Verify your OpenTelemetry collector is properly configured and accessible

## Technical Details

The OpenTelemetry Auto-Injector works by:
1. **Library Preloading**: Using `LD_PRELOAD` mechanism to load instrumentation before application libraries
2. **Symbol Interposition**: Intercepting calls to instrumented functions (HTTP clients, database drivers, etc.)
3. **Dynamic Instrumentation**: Automatically wrapping function calls with OpenTelemetry spans
4. **Context Propagation**: Maintaining trace context across process boundaries and async operations
5. **Data Export**: Sending telemetry data to configured OTLP endpoints

This approach provides zero-code instrumentation but requires careful system configuration and understanding of its system-wide impact.

---