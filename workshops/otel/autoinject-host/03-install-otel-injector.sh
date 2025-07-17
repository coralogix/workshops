#!/bin/bash

set -u
set -o pipefail

SCRIPT_SHOULD_CONTINUE=true
stop_script() {
    local message=${1:-"Script execution stopped."}
    SCRIPT_SHOULD_CONTINUE=false
}

# Check to see if the user is root
IS_ROOT=false

if [ "${EUID:-}" = "0" ]; then
    IS_ROOT=true
fi

if [ "$IS_ROOT" = "false" ] && command -v id >/dev/null 2>&1; then
    if [ "$(id -u 2>/dev/null)" = "0" ]; then
        IS_ROOT=true
    fi
fi

if [ "$IS_ROOT" = "false" ] && command -v whoami >/dev/null 2>&1; then
    if [ "$(whoami 2>/dev/null)" = "root" ]; then
        IS_ROOT=true
    fi
fi

if [ "$IS_ROOT" = "false" ]; then
    stop_script
fi

if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

# Check to see if the package exists
PACKAGE_NAME="opentelemetry-injector.deb"

if [ ! -f "$PACKAGE_NAME" ]; then
    stop_script
fi

if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

# Configure instrumentation and install the instrumentaion package
show_installation_options() {
    echo "1) System-wide (affects all processes)"
    echo "2) Systemd services only"
}

install_system_wide() {
    CONFIG_DIR="/etc/opentelemetry/otelinject"
    sudo mkdir -p "$CONFIG_DIR"
    
    if [ ! -f "$CONFIG_DIR/java.conf" ]; then
        echo 'JAVA_TOOL_OPTIONS=-javaagent:/usr/lib/opentelemetry/javaagent.jar' | sudo tee "$CONFIG_DIR/java.conf" > /dev/null
    fi

    if [ ! -f "$CONFIG_DIR/node.conf" ]; then
        echo 'NODE_OPTIONS=-r /usr/lib/opentelemetry/otel-js/node_modules/@opentelemetry-js/otel/instrument' | sudo tee "$CONFIG_DIR/node.conf" > /dev/null
    fi

    if [ ! -f "$CONFIG_DIR/dotnet.conf" ]; then
        echo -e "CORECLR_ENABLE_PROFILING=1\nCORECLR_PROFILER={918728DD-259F-4A6A-AC2B-B85E1B658318}\nCORECLR_PROFILER_PATH=/usr/lib/opentelemetry/otel-dotnet/linux-x64/OpenTelemetry.AutoInstrumentation.Native.so\nDOTNET_ADDITIONAL_DEPS=/usr/lib/opentelemetry/otel-dotnet/AdditionalDeps\nDOTNET_SHARED_STORE=/usr/lib/opentelemetry/otel-dotnet/store\nDOTNET_STARTUP_HOOKS=/usr/lib/opentelemetry/otel-dotnet/net/OpenTelemetry.AutoInstrumentation.StartupHook.dll\nOTEL_DOTNET_AUTO_HOME=/usr/lib/opentelemetry/otel-dotnet" | sudo tee "$CONFIG_DIR/dotnet.conf" > /dev/null
    fi
    
    OTEL_LIB="/usr/lib/opentelemetry/libotelinject.so"
    if [ -f "$OTEL_LIB" ]; then
        if ! grep -Fxq "$OTEL_LIB" /etc/ld.so.preload 2>/dev/null; then
            echo "$OTEL_LIB" | sudo tee -a /etc/ld.so.preload > /dev/null
        fi
    else
        stop_script
        return 2>/dev/null || true
    fi
}

install_systemd_only() {
    SYSTEMD_DROPIN_DIR="/usr/lib/systemd/system.conf.d"
    sudo mkdir -p "$SYSTEMD_DROPIN_DIR"
    
    SAMPLE_FILE="/usr/lib/opentelemetry/examples/systemd/00-opentelemetry-injector.conf"
    TARGET_FILE="$SYSTEMD_DROPIN_DIR/00-opentelemetry-injector.conf"
    
    if [ -f "$SAMPLE_FILE" ]; then
        sudo cp "$SAMPLE_FILE" "$TARGET_FILE"
    else
        cat << 'EOF' | sudo tee "$TARGET_FILE" > /dev/null
[Manager]
DefaultEnvironment=LD_PRELOAD=/usr/lib/opentelemetry/libotelinject.so
EOF
    fi
}

if ! sudo dpkg -i "$PACKAGE_NAME"; then
    stop_script
fi

if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

show_installation_options
read -p "Enter your choice (1 or 2): " choice

case $choice in
    1)
        INSTALL_TYPE="system-wide"
        ;;
    2)
        INSTALL_TYPE="systemd-only"
        ;;
    *)
        stop_script
        ;;
esac

if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

case $INSTALL_TYPE in
    "system-wide")
        install_system_wide
        ;;
    "systemd-only")
        install_systemd_only
        ;;
esac 