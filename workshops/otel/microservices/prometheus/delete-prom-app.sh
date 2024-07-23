kubectl delete deployment prometheus-client1 --ignore-not-found=true &
kubectl delete deployment prometheus-client2 --ignore-not-found=true &
kubectl delete deployment prometheus-client --ignore-not-found=true &