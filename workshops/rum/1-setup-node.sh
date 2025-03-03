# Required to avoid npm issues
unset NODE_OPTIONS

npm init -y

# Install OpenTelemetry dependencies
npm install --save @opentelemetry/api
npm install --save @opentelemetry/auto-instrumentations-node

# Install Coralogix SDK (force latest version)
npm i @coralogix/browser --force
# npm i @coralogix/browser@latest  # Alternative if force isn't needed

# Install core backend dependencies
npm install express pino cors node-fetch uuid

# Install Webpack & Babel for frontend bundling
npm install --save-dev webpack webpack-cli webpack-dev-server \
  babel-loader @babel/core @babel/preset-env \
  html-webpack-plugin clean-webpack-plugin
