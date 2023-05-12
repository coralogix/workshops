export PYTHON_TEST_URLGOOD=GOOD
# envsubst < deploy.yaml  | kubectl apply -f -
kubectl apply -f deploy.yaml