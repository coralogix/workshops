helm repo add jetstack https://charts.jetstack.io --force-update

helm install cert-manager jetstack/cert-manager   --namespace cert-manager   --create-namespace   --version v1.15.3   --set crds.enabled=true

helm repo add open-telemetry https://open-telemetry.github.io/opentelemetry-helm-charts

# Start the operator and enable Go support
helm install opentelemetry-operator open-telemetry/opentelemetry-operator \
	--namespace default \
	--set "manager.collectorImage.repository=otel/opentelemetry-collector-k8s" \
	--set manager.extraArgs={"--enable-go-instrumentation=true"}
