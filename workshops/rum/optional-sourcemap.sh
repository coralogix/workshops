sudo npm install -g @coralogix/rum-cli --force
coralogix-rum-cli upload-source-maps -k "your key" \
                  -a "steve.lerner@coralogix.com" \
                  -e "US2" \
                  -v "v1" \
                  -f "./dist"