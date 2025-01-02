// Import required modules
const http = require('http');
const { v4: uuidv4 } = require('uuid');
const pino = require('pino');

// Configuration
const PORT = 3000;
const logger = pino(); // Initialize Pino logger
let requestCount = 0; // Request counter

// Start the HTTP server
function startServer() {
    const server = http.createServer((req, res) => {
        try {
            // Generate a unique ID for each request
            const requestId = uuidv4();
            const responseBody = { message: 'Hello from the Node server!', requestId };

            // Send response
            res.statusCode = 200;
            res.setHeader('Content-Type', 'application/json');
            res.end(JSON.stringify(responseBody));

            // Log request details
            logger.info({
                timestamp: new Date().toISOString(),
                requestId,
                method: req.method,
                path: req.url,
                statusCode: res.statusCode,
                responseBody,
            }, 'Request processed');
        } catch (err) {
            // Handle unexpected errors
            logger.error({ error: err.message }, 'Error processing request');
            res.statusCode = 500;
            res.end(JSON.stringify({ error: 'Internal Server Error' }));
        }
    });

    // Handle server errors
    server.on('error', (err) => {
        logger.error({ error: err.message }, 'Server encountered an error');
    });

    // Start listening on the configured port
    server.listen(PORT, () => {
        logger.info(`Server listening on port ${PORT}`);
    });
}

// HTTP client function with retry and timeout
function httpGetLocalServer(retries = 3) {
    if (requestCount >= 200) {
        logger.info('Completed 200 requests. Exiting application.');
        process.exit(); // Exit the application after 200 requests
    }
    requestCount++; // Increment request counter

    const requestId = uuidv4();
    const options = {
        hostname: 'localhost',
        port: PORT,
        path: '/',
        method: 'GET',
        headers: { 'X-Request-ID': requestId },
        timeout: 5000, // Set a timeout of 5 seconds
    };

    const req = http.request(options, (res) => {
        let data = '';
        res.on('data', (chunk) => {
            data += chunk;
        });
        res.on('end', () => {
            try {
                const responseBody = JSON.parse(data);
                logger.info({
                    timestamp: new Date().toISOString(),
                    requestId,
                    method: 'GET',
                    path: options.path,
                    statusCode: res.statusCode,
                    responseBody,
                }, 'Request received and processed');

                // Recursively make the next request with a slight delay
                setTimeout(httpGetLocalServer, 100 + Math.random() * 500);
            } catch (err) {
                logger.error({ error: err.message }, 'Error parsing response');
            }
        });
    });

    // Handle request errors
    req.on('error', (error) => {
        logger.error({ error: error.message }, 'Request failed');
        if (retries > 0) {
            logger.info(`Retrying... Attempts left: ${retries}`);
            setTimeout(() => httpGetLocalServer(retries - 1), 500);
        } else {
            logger.error('Max retries reached. Skipping request.');
        }
    });

    // Handle request timeout
    req.setTimeout(5000, () => {
        logger.warn({ requestId }, 'Request timed out');
        req.destroy();
    });

    req.end();
}

// Start the server and initiate requests
startServer();
setTimeout(httpGetLocalServer, 100); // Start the requesting loop with a slight delay