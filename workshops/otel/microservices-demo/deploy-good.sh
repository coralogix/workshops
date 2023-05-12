export PYTHON_TEST_URLGOOD=1
envsubst < deploy.yaml  | kubectl apply -f -
# for f in deploy.yaml; do envsubst < $f | kubectl apply -f -; done
# kubectl apply -f deploy-good.yaml