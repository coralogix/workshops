#!/bin/bash

echo "Deleting Java microservices resources..."

# Delete client deployment
kubectl delete deployment cx-java-client

# Delete server service
kubectl delete service cx-java-server-service

# Delete server deployment
kubectl delete deployment cx-java-server

echo "All Java microservices resources deleted."