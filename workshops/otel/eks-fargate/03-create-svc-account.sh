##!/bin/bash
CLUSTER_NAME=slerner-eks-fargate
REGION=us-west-2
SERVICE_ACCOUNT_NAMESPACE=cx-eks-fargate-otel
SERVICE_ACCOUNT_NAME=cx-otel-collector
SERVICE_ACCOUNT_IAM_ROLE=EKS-Fargate-cx-OTEL-ServiceAccount-Role
SERVICE_ACCOUNT_IAM_POLICY=arn:aws:iam::aws:policy/CloudWatchAgentServerPolicy

eksctl utils associate-iam-oidc-provider \
--cluster=$CLUSTER_NAME \
--approve

eksctl create iamserviceaccount \
--cluster=$CLUSTER_NAME \
--region=$REGION \
--name=$SERVICE_ACCOUNT_NAME \
--namespace=$SERVICE_ACCOUNT_NAMESPACE \
--role-name=$SERVICE_ACCOUNT_IAM_ROLE \
--attach-policy-arn=$SERVICE_ACCOUNT_IAM_POLICY \
--approve \
--override-existing-serviceaccounts

kubectl describe serviceaccount cx-otel-collector -n cx-eks-fargate-otel