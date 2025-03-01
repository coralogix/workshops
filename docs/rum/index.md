
# Real User Monitoring: Browser/Mobile

## Browser  

This example shows a front->back end tracing example app running on a Mac (can be run on Windows)  
A front end Javascript app will send RUM telemetry (logs) as well as opt-in trace spans directly to Coralogix  
Also the locally hosted Node back end app will trace spans through the localhost OpenTelemetry collector  

### Step 0 - Install an OpenTelemetry Collector on your Mac (Can work for Windows as well)  

Download a current release of the contrib OpenTelmetry Collector for your Mac (Apple Silicon are the `darwin_arm` releases)  
[https://github.com/open-telemetry/opentelemetry-collector-releases/releases](https://github.com/open-telemetry/opentelemetry-collector-releases/releases)  

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

0. Configure the Coralogix Exporter in `otelcol/otel-config.yaml` with your Coralogix key and domain  
Run the collector with in a **dedicated terminal**:  
```
./otel-contrib --config otel-config.yaml
```  
  
1. **Start a new terminal**

2. **Ensure Node.js is installed.**

3. **Add Coralogix RUM integration:**
   - In`src/index.js` add the Coralogix RUM `Browser SDK` and `User Context and Labels` at the top and make sure to include the commented trace capturing stanza such that it looks like:

   ```
   CoralogixRum.init({
      public_key: 'YOURKEY',
      application: 'YOURAPPNAME',
      version: 'YOURVERSION',
      coralogixDomain: 'YOURCORALOGIXDOMAIN',
      traceParentInHeader: {
         enabled: true,
         options: {
            propagateTraceHeaderCorsUrls: [new RegExp('http://localhost.*')],
         },
      },
   });
   ```  

4. **Set up node packages:**
   ```bash
   source 1-setup-node.sh
   ```

5. **Package files using webpack:**
   ```bash
   source 2-webpack.sh
   ```

6. **Edit the file to add your sourcemap key and region:**
   - This key is available in the RUM Integration -> SourceMap setup.

7. **Upload the sourcemap:**
   ```bash
   source 3-sourcemap.sh
   ```
8. **Setup the env variables for OpenTelemetry zero-code instrumentation of Node:**
   ```bash
   source 4-setup-otel-env-var.sh
   ```
9. **Run the Node Express web server:**
   ```bash
   source 5-node.sh
   ```
   - Open a browser to `http://localhost:3000` to load the app served by Node. Refresh a few times  
   - Study the RUM results in Coralogix including RUM logs, front/back end traces, APM of the Node back end, and RUM dashboards  
   - To stop the Express server and OpenTelmetry Collector, use `ctrl-c`  
  
10. **Optional: Clean up Node packages and webpack artifacts:**
   ```bash
   source 6-cleanup.sh
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
- React Native  
- iOS: Swift, UIKit  
  
### iOS Workshop
  
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
  
### React Native Workshop

Follow the instructions in the repo directory: [https://github.com/coralogix/workshops/tree/master/workshops/mobilerum/react](https://github.com/coralogix/workshops/tree/master/workshops/mobilerum/react)