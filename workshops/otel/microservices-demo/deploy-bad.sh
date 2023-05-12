export PYTHON_TEST_URLGOOD=BAD
envsubst < deploy.yaml  | kubectl apply -f -
# kubectl apply -f deploy.yaml