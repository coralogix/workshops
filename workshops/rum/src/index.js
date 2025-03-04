// This example assumes running on a mac with localhost mac otel collector to collect app spans
// Initialize Coralogix RUM first
// copy Coralogix RUM SDK from Integration here
// make sure to add the trace capturing at end of SDK stanza
// traceParentInHeader: {
//   enabled: true,
//   options: {
//     propagateTraceHeaderCorsUrls: [new RegExp('http://localhost.*')],
//   },

// Example from Coralogix RUM Integration:

// import { CoralogixRum } from '@coralogix/browser';
// CoralogixRum.init({
//   public_key: 'yourkeyhere',
//   application: 'yourappnamehere',
//   version: 'yourversionhere',
//   coralogixDomain: 'coralogixdomainhere',
//   traceParentInHeader: {
//     enabled: true,
//     options: {
//       propagateTraceHeaderCorsUrls: [new RegExp('.*')],
//     },
//   },
// });

// Import Coralogix RUM for monitoring

console.log('JavaScript file loaded.');

document.addEventListener('DOMContentLoaded', () => {
  console.log('DOM fully loaded');

  const appDiv = document.getElementById('app');

  if (appDiv) {
    // Inject styles dynamically
    const style = document.createElement('style');
    style.innerHTML = `
      body { font-family: Arial, sans-serif; margin: 20px; background-color: #f4f4f4; }
      header { background-color: #28a745; color: white; padding: 10px 20px; margin-bottom: 20px; }
      .container { background: white; padding: 20px; border-radius: 4px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
      .section { margin-top: 20px; }
      button { padding: 10px 15px; margin-right: 10px; border: none; background-color: #28a745; color: white; border-radius: 4px; cursor: pointer; }
      button:hover { background-color: #218838; }
      #greeting, #apiResponse, #errorStatus, #slowOpStatus { margin-top: 10px; font-size: 1.2em; color: #333; }
      #apiResponse, #errorStatus, #slowOpStatus { font-family: monospace; background: #eee; padding: 10px; border-radius: 4px; max-height: 200px; overflow-y: auto; }
      #errorStatus { color: #28a745; }
    `;
    document.head.appendChild(style);

    // Inject HTML structure dynamically
    appDiv.innerHTML = `
      <header><nav></nav></header>
      <div class="container">
        <h1>Welcome to Coralogix RUM Microservices Demo!</h1>

        <div class="section">
          <h2>User Greeting</h2>
          <form id="nameForm">
            <label for="nameInput">Enter your name:</label>
            <input type="text" id="nameInput" required />
            <button type="submit">Submit</button>
          </form>
          <div id="greeting"></div>
        </div>

        <div class="section">
          <h2>Error Simulation</h2>
          <p>Click to simulate a JavaScript error (sent to Coralogix RUM).</p>
          <button id="errorButton">Simulate Error</button>
          <div id="errorStatus"></div>
        </div>

        <div class="section">
          <h2>Slow Operation Simulation</h2>
          <button id="slowOpButton">Simulate Slow Operation</button>
          <div id="slowOpStatus"></div>
        </div>

        <div class="section">
          <h2>API Call to Backend</h2>
          <button id="apiButton">Fetch API Data</button>
          <div id="apiResponse"></div>
        </div>
      </div>
    `;

    console.log('UI injected successfully.');

    // Get Coralogix RUM Tracer
    const customTracer = CoralogixRum.getCustomTracer();

    // Greeting Form Submission
    document.getElementById('nameForm').addEventListener('submit', (event) => {
      event.preventDefault();
      
      const globalSpan = customTracer.startGlobalSpan('greeting-span', { action: 'submit' });

      globalSpan.withContext(() => {
        const customSpan = globalSpan.startCustomSpan('process-greeting', { action: 'submit' });
        const name = document.getElementById('nameInput').value;
        document.getElementById('greeting').innerText = `Hello, ${name}! Welcome to the Microservices Demo!`;
        customSpan.endSpan();
      });

      globalSpan.endSpan();
    });

    // Error Simulation
    document.getElementById('errorButton').addEventListener('click', () => {
      const globalSpan = customTracer.startGlobalSpan('error-span', { action: 'trigger-error' });

      globalSpan.withContext(() => {
        const customSpan = globalSpan.startCustomSpan('simulate-error', { errorType: 'manual' });
        document.getElementById('errorStatus').innerText = 'Simulated error triggered...';
        setTimeout(() => {
          nonExistentFunction(); // Simulated JS error
        }, 0);
        customSpan.endSpan();
      });

      globalSpan.endSpan();
    });

    // Slow Operation Simulation
    document.getElementById('slowOpButton').addEventListener('click', () => {
      const globalSpan = customTracer.startGlobalSpan('slow-operation-span', { action: 'computation' });

      globalSpan.withContext(() => {
        const customSpan = globalSpan.startCustomSpan('compute-sum', { load: 'high' });
        const statusDiv = document.getElementById('slowOpStatus');
        statusDiv.innerText = 'Processing...';
        setTimeout(() => {
          let sum = 0;
          for (let i = 0; i < 1e7; i++) {
            sum += i;
          }
          statusDiv.innerText = `Heavy computation complete. Result: ${sum}`;
          customSpan.endSpan();
        }, 100);
      });

      globalSpan.endSpan();
    });

    // API Call Simulation (Talks to `app.js`, which forwards to `service.js`)
    document.getElementById('apiButton').addEventListener('click', () => {
      const globalSpan = customTracer.startGlobalSpan('api-call-span', { action: 'fetch-data' });

      globalSpan.withContext(() => {
        const customSpan = globalSpan.startCustomSpan('fetch-api-data', { url: 'http://localhost:3000/api/data' });
        const apiDiv = document.getElementById('apiResponse');
        apiDiv.innerText = 'Fetching data...';

        fetch('http://localhost:3000/api/data')
          .then((response) => response.json())
          .then((data) => {
            apiDiv.innerText = JSON.stringify(data, null, 2);
            customSpan.endSpan();
          })
          .catch((error) => {
            apiDiv.innerText = `Error fetching data: ${error}`;
            customSpan.endSpan();
          });
      });

      globalSpan.endSpan();
    });

    console.log('Coralogix RUM initialized.');
  } else {
    console.error('Element with ID "app" not found.');
  }
});