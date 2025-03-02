
# Real User Monitoring

## Browser RUM Workshop

- This example shows a browser->front->back end tracing example app running on a Mac (can be run on Windows)  
- The browser will call a `frontend` service which then calls a `backend gateway` service and the RUM session trace ID will be propagated showing front-to-back tracing       
- The browser sesesion RUM trace will be sent directly to Coralogix and the locally hosted Node frontend/backend apps will send trace spans through the localhost OpenTelemetry collector to Coralogix  
- Logs will be printed to the console and not collected by the collector in this example- the focus is on tracing  

### Step 1 - Setup
Clone the repository:
```bash
git clone https://github.com/coralogix/workshops
```
  
### Step 2 - Install and run an OpenTelemetry Collector on your Mac (Can work for Windows as well)  

**Start a new terminal**- this must be run in a dedicated terminal (see screenshot below)      
Navigate to the `otelcol` directory
```bash
cd ./workshops/workshops/rum/otelcol
```
  
Download a current release of the contrib OpenTelmetry Collector for your Mac (Apple Silicon are the `darwin_arm` releases)  
[https://github.com/open-telemetry/opentelemetry-collector-releases/releases](https://github.com/open-telemetry/opentelemetry-collector-releases/releases) 
  
Configure the Coralogix Exporter in `otel-config.yaml` with your Coralogix key and domain  
```
./otel-contrib --config otel-config.yaml
```  
  
### Step 3 - Execute the RUM workshop
  
1. **Start a new terminal**- this must be run in a dedicated terminal (see screenshot below)   

2. **Ensure current Node.js and npm are installed**  
  
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
            propagateTraceHeaderCorsUrls: [new RegExp('.*')],
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
6. **Start the node backend gateway service**
   ```bash
   source 3-node-backend.sh
   ```
7. **Start the node frontend service**
   ```bash
   source 4-node-frontend.sh
   ```
Your screen should now look like the screenshot below- with Otel Collector in one terminal, back end in another, and frontend in another:  
<img src="https://coralogix.github.io/workshops/images/rum/vsc.png" width=540>    
  
8. **Open web browser to exercise RUM sesions**
Open browser to `http://localhost:3000`  
Try each option on the page to exercise RUM trace examples  
  
9. **Study the results in Coralogix** 
Web browser RUM trace- notice the traceID:  
<img src="https://coralogix.github.io/workshops/images/rum/rum-frontend-trace.png" width=540>     
  
Backend gateway service- the traceID is the same as the web browser RUM session ID- this demonstrates front->back tracing:  
<img src="https://coralogix.github.io/workshops/images/rum/rum-frontend-trace.png" width=540>   
  
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
  
### Mobile RUM Workshop: iOS  
  
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
- **Network requests:** Will send user session data while the app is running  
- **Exception:** Will crash the app with an exception. Stop Xcode and then run the app in the emulator so the app restarts and instrumentation sends crash analytics to Coralogix.  
- **Crash:** Similar to the exception above - crashes the app, stop Xcode, re-run the app to send analytics  
  
### React Native Workshop

Follow the instructions in the repo directory: [https://github.com/coralogix/workshops/tree/master/workshops/mobilerum/react](https://github.com/coralogix/workshops/tree/master/workshops/mobilerum/react)