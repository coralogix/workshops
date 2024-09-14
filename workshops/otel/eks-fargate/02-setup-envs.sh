kubectl create namespace cx-eks-fargate-otel
export CORALOGIX_DOMAIN=cx498.coralogix.com
export PRIVATE_KEY=YOURKEYHERE
export NAMESPACE=default
kubectl delete secret coralogix-keys -n $NAMESPACE
kubectl create secret generic coralogix-keys -n $NAMESPACE \
  --from-literal=PRIVATE_KEY=$PRIVATE_KEY \
  --from-literal=CORALOGIX_DOMAIN=cx498.coralogix.com
kubectl get secret coralogix-keys -o yaml -n $NAMESPACE