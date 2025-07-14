#!/bin/bash

# Script to run Java app as a systemd service
# This script creates a systemd service for the Java application

set -e

# Configuration variables
APP_NAME="java-app"
APP_USER="javaapp"
APP_GROUP="javaapp"
APP_DIR="/opt/java-app"
JAR_FILE="java-app-1.0-SNAPSHOT.jar"
SERVICE_NAME="${APP_NAME}.service"
LOG_DIR="/var/log/${APP_NAME}"
PID_DIR="/var/run/${APP_NAME}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if running as root
if [[ $EUID -ne 0 ]]; then
   print_error "This script must be run as root (use sudo)"
   exit 1
fi

print_status "Setting up Java application as systemd service..."

# Create application user and group
print_status "Creating application user and group..."
if ! id "$APP_USER" &>/dev/null; then
    useradd -r -s /bin/false -d "$APP_DIR" "$APP_USER"
    print_status "Created user: $APP_USER"
else
    print_warning "User $APP_USER already exists"
fi

# Create application directory
print_status "Creating application directory..."
mkdir -p "$APP_DIR"
chown "$APP_USER:$APP_GROUP" "$APP_DIR"

# Create log and PID directories
print_status "Creating log and PID directories..."
mkdir -p "$LOG_DIR" "$PID_DIR"
chown "$APP_USER:$APP_GROUP" "$LOG_DIR" "$PID_DIR"

# Copy JAR file to application directory
print_status "Copying JAR file to application directory..."
if [[ -f "target/$JAR_FILE" ]]; then
    cp "target/$JAR_FILE" "$APP_DIR/"
    chown "$APP_USER:$APP_GROUP" "$APP_DIR/$JAR_FILE"
    print_status "Copied $JAR_FILE to $APP_DIR/"
else
    print_error "JAR file not found at target/$JAR_FILE"
    print_status "Please run 'mvn clean package' first to build the application"
    exit 1
fi

# Create systemd service file
print_status "Creating systemd service file..."
cat > "/etc/systemd/system/$SERVICE_NAME" << EOF
[Unit]
Description=Java Application Service
After=network.target
Wants=network.target

[Service]
Type=simple
User=$APP_USER
Group=$APP_GROUP
WorkingDirectory=$APP_DIR
ExecStart=/usr/bin/java \\
    -Dlog4j.configurationFile=classpath:log4j2.xml \\
    -Dcom.sun.management.jmxremote \\
    -Dcom.sun.management.jmxremote.port=9999 \\
    -Dcom.sun.management.jmxremote.authenticate=false \\
    -Dcom.sun.management.jmxremote.ssl=false \\
    -jar $JAR_FILE
ExecReload=/bin/kill -HUP \$MAINPID
Restart=always
RestartSec=10
StandardOutput=journal
StandardError=journal
SyslogIdentifier=$APP_NAME
PIDFile=$PID_DIR/$APP_NAME.pid

# Environment variables
Environment="JAVA_OPTS=-Xms512m -Xmx1024m"

# Security settings
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=$APP_DIR $LOG_DIR $PID_DIR

[Install]
WantedBy=multi-user.target
EOF

print_status "Created systemd service file: /etc/systemd/system/$SERVICE_NAME"

# Reload systemd daemon
print_status "Reloading systemd daemon..."
systemctl daemon-reload

# Enable and start the service
print_status "Enabling and starting the service..."
systemctl enable "$SERVICE_NAME"
systemctl start "$SERVICE_NAME"

# Check service status
print_status "Checking service status..."
if systemctl is-active --quiet "$SERVICE_NAME"; then
    print_status "Service is running successfully!"
    print_status "Service status:"
    systemctl status "$SERVICE_NAME" --no-pager -l
else
    print_error "Service failed to start!"
    print_status "Checking service logs:"
    journalctl -u "$SERVICE_NAME" --no-pager -l --since "1 minute ago"
    exit 1
fi

print_status "Setup complete!"
print_status "Service management commands:"
echo "  Start service:   sudo systemctl start $SERVICE_NAME"
echo "  Stop service:    sudo systemctl stop $SERVICE_NAME"
echo "  Restart service: sudo systemctl restart $SERVICE_NAME"
echo "  Check status:    sudo systemctl status $SERVICE_NAME"
echo "  View logs:       sudo journalctl -u $SERVICE_NAME -f"
echo "  Disable service: sudo systemctl disable $SERVICE_NAME"
