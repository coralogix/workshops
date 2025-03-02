import express from 'express';
import path from 'path';
import pino from 'pino';
import cors from 'cors';
import fetch from 'node-fetch';
import { v4 as uuidv4 } from 'uuid';

const app = express();
const PORT = 3000;
const logger = pino(); // Create Pino logger instance

app.use(cors());
app.use(express.json());

// Middleware to assign a unique request ID
app.use((req, res, next) => {
    req.requestId = uuidv4();  // Generate unique request ID
    logger.info({ requestId: req.requestId, method: req.method, url: req.originalUrl }, 'Incoming request');
    next();
});

// API Route - Calls `service.js`
app.get('/api/data', async (req, res) => {
    try {
        const response = await fetch('http://localhost:4000/service-data', {
            headers: { 'X-Request-ID': req.requestId }, // Pass the request ID to `service.js`
        });
        const data = await response.json();
        
        logger.info({ requestId: req.requestId, serviceData: data }, 'Response from service.js');
        res.json({ from: 'app.js', requestId: req.requestId, serviceData: data });
    } catch (error) {
        logger.error({ requestId: req.requestId, error: error.message }, 'Error fetching data from service.js');
        res.status(500).json({ error: 'Error fetching service data' });
    }
});

// Serve frontend
app.use(express.static(path.join(process.cwd(), 'dist')));

// Serve `index.html` for SPA support
app.get('*', (req, res) => {
    res.sendFile(path.join(process.cwd(), 'dist', 'index.html'));
});

// Start the API Gateway
app.listen(PORT, () => {
    logger.info({ port: PORT }, 'API Gateway started');
});
