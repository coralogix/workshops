#!/bin/bash

NAMESPACE="default"  # Set your namespace
PVC_NAME="jmx-metrics-pvc"  # Set your PVC name

# Get the PV bound to the PVC
PV_NAME=$(kubectl get pvc $PVC_NAME -n $NAMESPACE -o jsonpath='{.spec.volumeName}')

if [ -z "$PV_NAME" ]; then
    echo "PVC $PVC_NAME not found or not bound to any PV."
    exit 1
fi

echo "Deleting PVC: $PVC_NAME in namespace: $NAMESPACE"
kubectl delete pvc $PVC_NAME -n $NAMESPACE

echo "Deleting PV: $PV_NAME"
kubectl delete pv $PV_NAME

echo "Cleanup complete!"
