export PYTHON_TEST_URLBAD=1
envsubst < deploy.yaml  | kubectl apply -f -
# kubectl apply -f deploy-good.yaml