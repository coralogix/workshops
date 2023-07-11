Work in process host example:  

Requires locally running otel collector correctly configured for Coralogix.  

#### Run Flask and Redis Server  
in `python` dir:
```
source setup.sh
source start-flask.sh
```

#### run Node client  
in `node` dir:
```
setup-node.sh
setup-node-env.sh
start-node.sh
```