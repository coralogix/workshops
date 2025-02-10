
# Real User Monitoring: Browser/Mobile

## Browser

### Step 1 - Setup
Clone the repository:
```bash
git clone https://github.com/coralogix/workshops
```

### Step 2 - Change to the workshop directory
Navigate to the proper directory for the workshop example:
```bash
cd ./workshops/workshops/rum
```

### Step 3 - Execute the workshop

1. **Ensure Node.js is installed.**

2. **Add Coralogix RUM integration:**
   - Create a new file called `src/index.js` and integrate the Coralogix RUM `Browser SDK` and `User Context and Labels`.

3. **Set up node packages:**
   ```bash
   source 1-setup-node.sh
   ```

4. **Package files using webpack:**
   ```bash
   source 2-webpack.sh
   ```

5. **Edit the file to add your sourcemap key and region:**
   - This key is available in the RUM Integration -> SourceMap setup.

6. **Upload the sourcemap:**
   ```bash
   source 3-sourcemap.sh
   ```

7. **Run the Node Express web server:**
   ```bash
   source 4-node.sh
   ```
   - Open a browser to `http://localhost:3000` to load the app served by Node. Refresh a few times.
   - Study the RUM results in Coralogix.
   - To stop the Express server, use `ctrl-c`.

8. **Optional: Clean up Node packages and webpack artifacts:**
   ```bash
   source 5-cleanup.sh
   ```

## Mobile

### Step 1 - Setup
Clone the repository:
```bash
git clone https://github.com/coralogix/workshops
```

### Step 2 - Change to the workshop directory
Navigate to the proper directory for the workshop example:
```bash
cd ./workshops/workshops/mobilerum
```

### Step 3 - The Workshops
The following examples are currently available:
- **React**  
- **iOS**  
  - Swift  
  - UIKit  

### iOS
  
When using this workshop, ensure that the `Coralogix Package Dependencies` are updated: right click on them and select `update`
  
Follow the [Coralogix SDK instructions](https://coralogix.com/docs/rum-ios-monitoring-setup/) and update the environment variable stanza according to your environment:

```swift
let options = CoralogixExporterOptions(
    coralogixDomain: CoralogixDomain.US2, // Set the Coralogix domain
    userContext: nil,                     // No user context provided
    environment: "PROD",                  // Environment set to production
    application: "CX-Demo-Swift",         // Name of the application
    version: "1",                         // Application version
    publicKey: "",                        // Public key for authentication
    ignoreUrls: [],                       // List of URLs to ignore
    ignoreErrors: [],                     // List of errors to ignore
    customDomainUrl: nil,                 // Custom domain URL (if any)
    labels: ["test": "example"],          // Additional labels for the RUM data
    debug: true                           // Debug mode enabled
)
```

Each project should build and run on an iPhone emulator (15 Pro Max was tested) and emit telemetry to the Coralogix RUM platform.

### Step 4 - Using the Demo App

There are three current example tests on the demo app:  
- **Network requests:** Will send user session data while the app is running.  
- **Exception:** Will crash the app with an exception. Stop Xcode and then run the app in the emulator so the app restarts and instrumentation sends crash analytics to Coralogix.  
- **Crash:** Similar to the exception above - crashes the app, stop Xcode, re-run the app to send analytics.  
  
### React

Follow the instructions in the repo directory: [https://github.com/coralogix/workshops/tree/master/workshops/mobilerum/react](https://github.com/coralogix/workshops/tree/master/workshops/mobilerum/react)