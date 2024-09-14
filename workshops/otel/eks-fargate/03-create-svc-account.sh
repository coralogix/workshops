#!/bin/bash
export CLUSTER_NAME=slerner-eksfargate
export REGION=us-west-2
export SERVICE_ACCOUNT_NAME=cx-otel-collector
export SERVICE_ACCOUNT_NAMESPACE=default  # Or your intended namespace
export SERVICE_ACCOUNT_IAM_ROLE=EKS-Fargate-cx-OTEL-ServiceAccount-Role
export SERVICE_ACCOUNT_IAM_POLICY=arn:aws:iam::aws:policy/CloudWatchAgentServerPolicy

# Associate OIDC provider with the cluster
eksctl utils associate-iam-oidc-provider \
  --cluster=$CLUSTER_NAME \
  --approve

# Create the IAM service account and associate the policy
eksctl create iamserviceaccount \
  --cluster=$CLUSTER_NAME \
  --region=$REGION \
  --name=$SERVICE_ACCOUNT_NAME \
  --namespace=$SERVICE_ACCOUNT_NAMESPACE \
  --role-name=$SERVICE_ACCOUNT_IAM_ROLE \
  --attach-policy-arn=$SERVICE_ACCOUNT_IAM_POLICY \
  --approve \
  --override-existing-serviceaccounts \
  --verbose 4

# Validate Service Account creation
kubectl get serviceaccount $SERVICE_ACCOUNT_NAME -n $SERVICE_ACCOUNT_NAMESPACE

# Validate IAM Role creation  
# aws iam get-role --role-name $SERVICE_ACCOUNT_IAM_ROLE