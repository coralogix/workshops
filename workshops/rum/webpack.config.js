const path = require('path');

module.exports = {
  // Entry point of your application
  entry: './src/index.js',

  // Output configuration
  output: {
    // Output directory as an absolute path
    path: path.resolve(__dirname, 'dist'),
    // Name of the output bundle
    filename: 'bundle.js'
  },

  // Generate a source map for debugging
  devtool: 'source-map',
};
