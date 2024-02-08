const http = require('http');
const { v4: uuidv4 } = require('uuid');
const pino = require('pino');

const PORT = 3000;
let requestCount = 0; // Initialize request counter
const logger = pino(); // Initialize Pino logger

function startServer() {
    const server = http.createServer((req, res) => {
        const requestId = uuidv4();
        const responseBody = { message: 'Hello from the Node server!', requestId };

        res.statusCode = 200;
        res.setHeader('Content-Type', 'application/json');
        res.end(JSON.stringify(responseBody));

        // Log the request and response details using Pino
        logger.info({
            timestamp: new Date().toISOString(),
            requestId,
            method: req.method,
            path: req.url,
            statusCode: res.statusCode,
            responseBody
        }, 'Request processed');
    });

    server.listen(PORT, () => {
        logger.info(`Server listening on port ${PORT}`);
    });
}

function httpGetLocalServer() {
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
        headers: {
            'X-Request-ID': requestId,
        },
    };

    const req = http.request(options, (res) => {
        let data = '';
        res.on('data', (chunk) => {
            data += chunk;
        });
        res.on('end', () => {
            const responseBody = JSON.parse(data);
            // Log the request and response details using Pino
            logger.info({
                timestamp: new Date().toISOString(),
                requestId,
                method: 'GET',
                path: options.path,
                statusCode: res.statusCode,
                responseBody
            }, 'Request received and processed');

            // Recursively make the next request with a slight delay
            setTimeout(httpGetLocalServer, 100);
        });
    });

    req.on('error', (error) => {
        logger.error(error);
    });

    req.end();
}

startServer();
setTimeout(httpGetLocalServer, 100); // Start the requesting loop with a slight delay