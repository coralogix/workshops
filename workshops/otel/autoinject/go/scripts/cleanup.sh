#!/bin/bash

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Cleaning up Go OpenTelemetry Auto-Injection Demo...${NC}"

# Get the directory of this script
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
K8S_DIR="$PROJECT_ROOT/k8s"

echo -e "${BLUE}Removing client deployment...${NC}"
kubectl delete -f "$K8S_DIR/client-deployment.yaml" --ignore-not-found=true

echo -e "${BLUE}Removing server deployment...${NC}"
kubectl delete -f "$K8S_DIR/server-deployment.yaml" --ignore-not-found=true

echo -e "${BLUE}Removing instrumentation configuration...${NC}"
kubectl delete -f "$K8S_DIR/instrumentation.yaml" --ignore-not-found=true

echo -e "${BLUE}Removing namespace...${NC}"
kubectl delete -f "$K8S_DIR/namespace.yaml" --ignore-not-found=true

echo -e "${GREEN}Cleanup completed!${NC}"

# Optional: Remove Docker images
read -p "Do you want to remove Docker images? (y/n): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo -e "${BLUE}Removing Docker images...${NC}"
    docker rmi go-server:latest go-client:latest 2>/dev/null || true
    echo -e "${GREEN}Docker images removed!${NC}"
fi 