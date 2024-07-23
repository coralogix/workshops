const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');

module.exports = {
  // Set the mode to 'development' or 'production'
  mode: 'development', // or 'production'

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

  // Plugins
  plugins: [
    new HtmlWebpackPlugin({
      template: './src/index.html', // Source HTML file
      filename: 'index.html'        // Output HTML file
    })
  ],
};
