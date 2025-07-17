#!/bin/bash

# Maven Installation Script for Ubuntu
# This script installs Apache Maven on Ubuntu systems

set -e  # Exit on any error

# Global flag to control script execution - prevents terminal crashes when sourced
SCRIPT_SHOULD_CONTINUE=true

# Function to stop script execution without exiting terminal
# This is safer than using 'exit' which could kill terminal sessions
# Args: $1 - Optional custom message (defaults to generic stop message)
stop_script() {
    local message=${1:-"Script execution stopped."}
    echo "$message"
    SCRIPT_SHOULD_CONTINUE=false
}

# Configuration
MAVEN_VERSION="3.9.6"
MAVEN_URL="https://archive.apache.org/dist/maven/maven-3/${MAVEN_VERSION}/binaries/apache-maven-${MAVEN_VERSION}-bin.tar.gz"
INSTALL_DIR="/opt"
MAVEN_HOME="${INSTALL_DIR}/apache-maven-${MAVEN_VERSION}"

echo "Starting Maven ${MAVEN_VERSION} installation..."

# Check if running as root or with sudo
if [[ $EUID -eq 0 ]]; then
   echo "Running as root"
else
   echo "This script should be run with sudo privileges"
   stop_script
fi

# Early termination check - stop here if root validation failed
if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

# Check if Maven is already installed
echo "Checking for existing Maven installation..."
if command -v mvn &> /dev/null; then
    echo "Maven is already installed:"
    mvn -version
    echo ""
    echo "Current Maven location: $(which mvn)"
    echo ""
    read -p "Do you want to continue with the installation anyway? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "Installation cancelled."
        stop_script
    fi
    echo "Proceeding with installation..."
fi

# Early termination check
if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

# Check if Maven directory already exists
if [ -d "${MAVEN_HOME}" ]; then
    echo "Maven directory ${MAVEN_HOME} already exists."
    read -p "Do you want to remove the existing installation and reinstall? (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        echo "Removing existing Maven installation..."
        rm -rf "${MAVEN_HOME}"
        if [ -L "${INSTALL_DIR}/maven" ]; then
            rm -f "${INSTALL_DIR}/maven"
        fi
    else
        echo "Installation cancelled."
        stop_script
    fi
fi

# Early termination check
if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

# Update package list
echo "Updating package list..."
apt-get update

# Install required dependencies
echo "Installing required dependencies..."
apt-get install -y wget curl openjdk-8-jdk

# Check if Java is installed
if ! command -v java &> /dev/null; then
    echo "Java is not installed. Installing OpenJDK 8..."
    apt-get install -y openjdk-8-jdk
else
    echo "Java is already installed:"
    java -version
fi

# Create installation directory
echo "Creating installation directory..."
mkdir -p ${INSTALL_DIR}

echo "Downloading Maven ${MAVEN_VERSION}..."
cd /tmp
wget -O maven.tar.gz ${MAVEN_URL}

# Verify download
if [ ! -f maven.tar.gz ]; then
    echo "Failed to download Maven"
    stop_script
fi

# Early termination check
if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

# Extract Maven
echo "Extracting Maven..."
tar -xzf maven.tar.gz -C ${INSTALL_DIR}

# Remove downloaded archive
rm maven.tar.gz

# Set permissions
echo "Setting permissions..."
chown -R root:root ${MAVEN_HOME}
chmod -R 755 ${MAVEN_HOME}

# Create symlink for easier access
echo "Creating symlink..."
ln -sf "${MAVEN_HOME}" "${INSTALL_DIR}/maven"

# Add Maven to PATH in /etc/profile.d for immediate use
echo "Creating profile script for Maven..."
cat > /etc/profile.d/maven.sh << EOF
#!/bin/bash
export MAVEN_HOME=${MAVEN_HOME}
export PATH=\$PATH:\$MAVEN_HOME/bin
EOF

chmod +x /etc/profile.d/maven.sh

# Source the profile for current session
source /etc/profile.d/maven.sh

# Verify installation
echo "Verifying Maven installation..."
if command -v mvn &> /dev/null; then
    echo "Maven installation successful!"
    echo "Maven version:"
    mvn -version
    echo ""
    echo "Maven is installed at: ${MAVEN_HOME}"
    echo "To use Maven in new terminal sessions, restart your terminal or run:"
    echo "source /etc/profile.d/maven.sh"
else
    echo "Maven installation failed!"
    stop_script
fi

# Early termination check
if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

echo "Maven ${MAVEN_VERSION} installation completed successfully!" 