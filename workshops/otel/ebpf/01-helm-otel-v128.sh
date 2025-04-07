helm repo add coralogix https://cgx.jfrog.io/artifactory/coralogix-charts-virtual
helm repo update
helm upgrade --install otel-coralogix-integration coralogix/otel-integration --version=0.0.128 --render-subchart-notes --set global.domain="cx498.coralogix.com" --set global.clusterName="slerner-cluster"