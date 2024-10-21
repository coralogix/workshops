export PRIVATE_KEY=""
export NAMESPACE=cx-eks-fargate-otel
export CORALOGIX_DOMAIN=cx498.coralogix.com
kubectl create namespace cx-eks-fargate-otel
kubectl create secret generic coralogix-keys -n $NAMESPACE --from-literal=PRIVATE_KEY=$PRIVATE_KEY
kubectl get secret coralogix-keys -o yaml -n $NAMESPACE