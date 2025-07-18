#!/bin/bash

# Script to build and push Go auto-inject container for x86/amd64 (Kubernetes compatible)
# Requires Docker Hub login: docker login

set -e  # Exit on any error

echo "🔨 Building Docker image for x86/amd64 (Kubernetes compatible)..."
docker build --platform linux/amd64 . -t autoinject-go-x86

echo "🏷️  Tagging image for Docker Hub..."
docker tag autoinject-go-x86 dambott2/go-autoinject:latest

echo "📤 Pushing to Docker Hub..."
docker push dambott2/go-autoinject:latest

echo "✅ Successfully built and pushed dambott2/go-autoinject:latest"
echo "🚀 This image is now compatible with x86/amd64 Kubernetes clusters"
echo "💡 Deploy with: kubectl run go-app --image=dambott2/go-autoinject:latest --port=8080"
echo "💡 Or run locally: docker run -p 8080:8080 dambott2/go-autoinject:latest"
