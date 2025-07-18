#!/bin/bash

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Building Go OpenTelemetry Auto-Injection Demo Images...${NC}"

# Get the directory of this script
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

cd "$PROJECT_ROOT"

echo -e "${YELLOW}Creating go.sum...${NC}"
go mod tidy

echo -e "${YELLOW}Building server image...${NC}"
docker build -f Dockerfile.server -t go-server:latest .

echo -e "${YELLOW}Building client image...${NC}"
docker build -f Dockerfile.client -t go-client:latest .

echo -e "${GREEN}Build completed successfully!${NC}"
echo -e "${GREEN}Images created:${NC}"
echo -e "  - go-server:latest"
echo -e "  - go-client:latest"

echo -e "${YELLOW}To load images into kind cluster (if using kind):${NC}"
echo -e "  kind load docker-image go-server:latest"
echo -e "  kind load docker-image go-client:latest" 