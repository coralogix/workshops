helm repo add jaegertracing https://jaegertracing.github.io/helm-charts
helm repo update

helm install jaeger jaegertracing/jaeger \
  --namespace jaeger \
  --create-namespace \
  --set agent.enabled=false \
  --set collector.enabled=true \
  --set query.enabled=true \
  --set storage.type=memory

