import express from 'express';
import pino from 'pino';

const app = express();
const PORT = 4000;
const logger = pino(); // Create Pino logger instance

app.use(express.json());

// Middleware to log requests with request ID from `app.js`
app.use((req, res, next) => {
    const requestId = req.headers['x-request-id'] || 'N/A'; // Get request ID from `app.js`
    logger.info({ requestId, method: req.method, url: req.originalUrl }, 'Incoming request');
    req.requestId = requestId; // Store request ID for further use
    next();
});

// API endpoint that provides service data
app.get('/service-data', (req, res) => {
    const responseData = { from: 'service.js', requestId: req.requestId, message: 'This is data from the microservice' };
    logger.info({ requestId: req.requestId, responseData }, 'Responding with service data');
    res.json(responseData);
});

// Start microservice
app.listen(PORT, () => {
    logger.info({ port: PORT }, 'Microservice started');
});
