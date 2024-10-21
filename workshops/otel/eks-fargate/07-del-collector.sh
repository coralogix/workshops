#!/bin/bash

# Set the namespace variable

echo "Deleting StatefulSet..."
kubectl delete statefulset cx-otel-collector -n $NAMESPACE

echo "Deleting Service..."
kubectl delete service cx-otel-collector-service -n $NAMESPACE

echo "Deleting ConfigMaps..."
kubectl delete configmap cx-otel-collector-config -n $NAMESPACE
kubectl delete configmap label-node-script -n $NAMESPACE

echo "Deleting ClusterRole and ClusterRoleBinding..."
kubectl delete clusterrole cx-otel-collector-admin-role
kubectl delete clusterrolebinding cx-otel-collector-admin-role-binding

# Uncomment the following block if you want to delete the namespace as well
# echo "Deleting Namespace..."
# kubectl delete namespace $NAMESPACE

echo "All specified resources deleted."
