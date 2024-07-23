#!/bin/bash

echo "Clearing npm cache..."
npm cache clean --force

echo "Removing npm cache folder..."
rm -rf ~/.npm

echo "Removing node_modules and package-lock.json..."
rm -rf node_modules
rm -f package-lock.json

# echo "Reinstalling dependencies..."
# npm install

echo "Clearing npx cache..."
rm -rf ~/.npm/_npx

if command -v yarn &> /dev/null
then
    echo "Clearing Yarn cache..."
    yarn cache clean
else
    echo "Yarn is not installed, skipping Yarn cache clean."
fi

echo "Clearing Node.js REPL history..."
rm -f ~/.node_repl_history

echo "All Node.js and npm caches cleared successfully."
