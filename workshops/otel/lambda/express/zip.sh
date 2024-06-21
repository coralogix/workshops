rm -rf *.json
source npm-install.sh
rm function.zip
zip -r function.zip index.js node_modules package.json