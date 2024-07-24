# Real User Monitoring: Browser/Mobile

## Browser

### Step 1 - Setup
Clone repo:
```
git clone https://github.com/coralogix/workshops
```  

### Step 2 - Change to workshop dir
Change to the proper directory for workshop example:  

```
cd ./workshops/workshops/rum
```  

### Step 3 - Execute workshop  

- Current Node.js must be installed
- Add Coralogix RUM integration `Browser SDK` and `User Context and Labels` to a new file called `src/index.js`  
- Set up node packages:
```
source 1-setup-node.sh
```
- Run webpack to package the files in `dist` into a single `dist/main.js` file
```
source 2-webpack.sh
```
- Edit this file and add your sourcemap key and region. This key is available in the RUM Integration->SourceMap setup   
- Run this command to upload sourcemap  
```
source 3-sourcemap.sh
``` 
- Run the app.js node app. This will run the node Express web server in the terminal- leave it running.
```
source 4-node.sh
```
- Open a browser to `http://localhost:3000` to load the app served by Node. Refresh a few times  
- Study RUM results in Coralogix  
- `ctrl-c` to stop Express server
- Optional: `source 5-cleanup.sh` removes all Node packages and webpack artifacts  

## Mobile

### Step 1 - Setup
Clone repo:
```
git clone https://github.com/coralogix/workshops
```  

### Step 2 - Change to workshop dir
Change to the proper directory for workshop example:  

```
cd ./workshops/workshops/mobilerum
```  

### Step 3 - The Workshops

The following examples are currently available:

**iOS:**  
Swift    
UIKit

Each work the same way- follow the [Coralogix SDK instructions here](https://coralogix.com/docs/rum-ios-monitoring-setup/)  

and then update the environment variable stanza according to your environment:

```
        let options = CoralogixExporterOptions(
            coralogixDomain: CoralogixDomain.US2, // Set the Coralogix domain
            userContext: nil,                     // No user context provided
            environment: "PROD",                  // Environment set to production
            application: "CX-Demo-Swift",   // Name of the application
            version: "1",                         // Application version
            publicKey: "", // Public key for authentication
            ignoreUrls: [],                       // List of URLs to ignore
            ignoreErrors: [],                     // List of errors to ignore
            customDomainUrl: nil,                  // Custom domain URL (if any)
            labels: ["test": "example"],          // Additional labels for the RUM data
            debug: true                          // Debug mode disabled
        )
```

Each project should build and run on an iPhone emulator (15 Pro Max was tested) and emit telemetry to the Coralogix RUM platform.  

### Step 4 - Using the Demo App

There are three current example tests on the demo app:

- Network requests will send user session data while app is running  
- Exception- will crash the app with an exception. Stop Xcode and then run app in emulator so app restarts and instrumentation sends crash analytics to Coralogix
- Crash- like exception above- crahes app, stop Xcode, re-run app to send analytics  