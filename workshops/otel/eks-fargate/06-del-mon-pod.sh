#!/bin/bash

# Use the NAMESPACE environment variable, or set a default if not provided
NAMESPACE=${NAMESPACE:-cx-eks-fargate-otel}

echo "Deleting Deployment in namespace $NAMESPACE..."
kubectl delete deployment cx-otel-collector-monitor -n $NAMESPACE

echo "Deleting ConfigMap in namespace $NAMESPACE..."
kubectl delete configmap cx-otel-collector-monitor-config -n $NAMESPACE

# Optionally, delete the namespace if it's no longer needed
# Uncomment the following block if you want to delete the namespace as well
# echo "Deleting Namespace $NAMESPACE..."
# kubectl delete namespace $NAMESPACE

echo "All specified resources in namespace $NAMESPACE deleted."
