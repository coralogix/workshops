helm repo add coralogix https://cgx.jfrog.io/artifactory/coralogix-charts-virtual
helm repo update
kubectl create secret generic coralogix-keys --from-literal=PRIVATE_KEY=${CORALOGIX_API_KEY} -n default

helm upgrade --install otel-coralogix-integration coralogix/otel-integration  \
		--namespace default \
		--render-subchart-notes --set global.domain="cx498.coralogix.com" \
	       	--set global.clusterName="mycluster" 
