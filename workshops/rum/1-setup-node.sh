unset NODE_OPTIONS
npm init -y
npm install --save @opentelemetry/api
npm install --save @opentelemetry/auto-instrumentations-node
npm i @coralogix/browser --force
# npm i @coralogix/browser@latest
npm install express
npm install pino
npm install uuid
npm install --save-dev webpack webpack-cli webpack-dev-server babel-loader @babel/core @babel/preset-env html-webpack-plugin clean-webpack-plugin