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

### Step 3: Uninstall OpenTelemetry Auto-Injector (02-uninstall-autoinject-host.sh)

This script removes the OpenTelemetry Injector package and cleans up associated files.

```bash
sudo ./02-uninstall-autoinject-host.sh
```

**Important:** This script carefully removes only the OpenTelemetry Injector-specific line from `/etc/ld.so.preload` to avoid affecting other system components. It does **not** uninstall Docker or `build-essential` installed by the setup script, as those are considered broader system dependencies.

## Troubleshooting

*   **"This script must be run as root"**: Ensure you are running the scripts with `sudo`.
*   **Docker commands not working after setup**: Remember to log out and log back in after running `00-setup-env.sh` to apply the `docker` group changes.
*   **Installation/Uninstallation errors**: Check the output for specific error messages. Ensure you have an active internet connection.

---