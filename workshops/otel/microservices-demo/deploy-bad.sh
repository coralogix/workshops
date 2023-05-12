export PYTHON_TEST_URLBAD=TRUE
envsubst < deploy.yaml  | kubectl apply -f -
# kubectl apply -f deploy-good.yaml