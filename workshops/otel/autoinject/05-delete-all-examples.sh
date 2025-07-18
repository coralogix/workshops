NAMESPACE=default
kubectl delete deployment cx-autoinject-dotnet8 -n $NAMESPACE
kubectl delete deployment cx-autoinject-java -n $NAMESPACE
kubectl delete deployment cx-autoinject-node -n $NAMESPACE
kubectl delete deployment cx-autoinject-py -n $NAMESPACE
kubectl delete deployment cx-autoinject-go -n $NAMESPACE
kubectl delete service cx-autoinject-py -n $NAMESPACE
