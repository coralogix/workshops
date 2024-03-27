### 3 Minute RUM Workshop  
#### This can be run from a personal computer  

- Current Node.js must be installed
- Add Coralogix RUM integration `Browser SDK` and `User Context and Labels` to a new file called `dist/index.js`  
- Set up node packages:
```
source 1-setup-node.sh
```
- Run webpack to package the files in `dist` into a single `dist/main.js` file
```
source 2-webpack.sh
```
- Run the app.js node app. This will run the node Express web server in the terminal- leave it running.
```
source 3-node.sh
```
- Open a browser to `http://localhost:3000` to load the app served by Node. Refresh a few times  
- Study RUM results in Coralogix  
- `ctrl-c` to stop Express server
- Optional: `source 4-cleanup.sh` removes all Node packages and webpack artifacts  

