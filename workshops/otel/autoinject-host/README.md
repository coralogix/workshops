# OpenTelemetry Auto-Inject Host Setup

This directory contains a set of Bash scripts to automate the setup, installation, and uninstallation of the OpenTelemetry Auto-Injector on a host system. These scripts are designed for Ubuntu/Debian-based distributions.

## Scripts Overview

*   `00-setup-env.sh`: Sets up the necessary environment, including installing `build-essential` and Docker.
*   `01-install-autoinject-host.sh`: Clones the OpenTelemetry Injector repository, builds the Debian package, installs it, and configures the `ld.so.preload` mechanism.
*   `02-uninstall-autoinject-host.sh`: Uninstalls the OpenTelemetry Injector package and cleans up related files and configurations.

## Prerequisites

*   An Ubuntu/Debian-based operating system.
*   `sudo` privileges.
*   Internet connectivity to download packages and clone repositories.

## Usage

Follow the steps below to set up your environment, install the Auto-Injector, and optionally uninstall it.

### Step 1: Set up the Environment (00-setup-env.sh)

This script installs essential build tools and Docker. It adds your current user to the `docker` group, which requires a re-login to take effect.

```bash
sudo ./00-setup-env.sh
```

**Important:** After running this script, you must **log out and log back in** for the Docker group changes to apply. You can then verify your Docker installation by running `docker run hello-world`.

### Step 2: Install OpenTelemetry Auto-Injector (01-install-autoinject-host.sh)

This script downloads, builds, and installs the OpenTelemetry Injector. It will clone the `opentelemetry-injector` repository, build the Debian package, install it via `dpkg`, and add the `libotelinject.so` library to `/etc/ld.so.preload` for automatic instrumentation.

```bash
sudo ./01-install-autoinject-host.sh
```

**Note:**
*   The script will remove any existing `opentelemetry-injector` directory before cloning to ensure a clean build.
*   The OpenTelemetry Injector configuration can be found and updated in `/etc/opentelemetry/otelinject`.

### Step 3: Install and Run Sample Applications

This directory includes three sample applications to demonstrate the OpenTelemetry Auto-Injector in action. Each application is located in its own subdirectory and can be installed and run independently.

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

### Step 4: Uninstall OpenTelemetry Auto-Injector (02-uninstall-autoinject-host.sh)

This script removes the OpenTelemetry Injector package and cleans up associated files.

```bash
sudo ./02-uninstall-autoinject-host.sh
```

**Important:** This script carefully removes only the OpenTelemetry Injector-specific line from `/etc/ld.so.preload` to avoid affecting other system components. It does **not** uninstall Docker or `build-essential` installed by the setup script, as those are considered broader system dependencies.

## Application Verification

After installing the applications, you can verify that the OpenTelemetry Auto-Injector is working by:

1. **Checking application logs** for OpenTelemetry traces
2. **Monitoring the applications** using your configured OpenTelemetry backend
3. **Running the verification script** (if available):
   ```bash
   cd java
   ./verify-autoinject.sh
   ```

## Troubleshooting

*   **"This script must be run as root"**: Ensure you are running the scripts with `sudo`.
*   **Docker commands not working after setup**: Remember to log out and log back in after running `00-setup-env.sh` to apply the `docker` group changes.
*   **Installation/Uninstallation errors**: Check the output for specific error messages. Ensure you have an active internet connection.
*   **Application-specific issues**: Check the individual application directories for specific troubleshooting information.

---