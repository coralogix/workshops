#!/bin/bash

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Deploying Go OpenTelemetry Auto-Injection Demo in 'both' mode...${NC}"

# Get the directory of this script
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

echo -e "${BLUE}Deploying combined server/client application...${NC}"
kubectl apply -f "$PROJECT_ROOT/deploy-go-autoinject.yaml"

echo -e "${GREEN}Deployment completed!${NC}"
echo
echo -e "${YELLOW}Useful commands:${NC}"
echo -e "  # Check deployment status"
echo -e "  kubectl get pods -n default -l name=cx-autoinject-go"
echo
echo -e "  # View application logs"
echo -e "  kubectl logs -n default -l name=cx-autoinject-go -f"
echo
echo -e "  # Get service information"
echo -e "  kubectl get svc -n default cx-autoinject-go"
echo
echo -e "  # Port forward to access application"
echo -e "  kubectl port-forward -n default svc/cx-autoinject-go 8080:8080"
echo
echo -e "  # Test the application"
echo -e "  curl http://localhost:8080/health"

# Wait for pods to be ready
echo -e "${BLUE}Waiting for pods to be ready...${NC}"
kubectl wait --for=condition=ready pod -n default -l name=cx-autoinject-go --timeout=120s

echo -e "${GREEN}Application is ready!${NC}"
echo -e "${BLUE}Testing health endpoint...${NC}"

# Get the service cluster IP and test internally
SVC_IP=$(kubectl get svc -n default cx-autoinject-go -o jsonpath='{.spec.clusterIP}')
if [ ! -z "$SVC_IP" ]; then
    echo -e "${BLUE}Service available at: $SVC_IP:8080${NC}"
fi 