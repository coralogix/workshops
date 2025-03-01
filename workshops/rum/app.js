const express = require('express');
const path = require('path');
const pino = require('pino');
const { v4: uuidv4 } = require('uuid');

const app = express();
const PORT = 3000;
const logger = pino();

// Middleware for logging requests
app.use((req, res, next) => {
    const requestId = uuidv4();
    req.requestId = requestId;
    const startTime = Date.now();

    res.on('finish', () => {
        const duration = Date.now() - startTime;
        logger.info({
            timestamp: new Date().toISOString(),
            requestId,
            method: req.method,
            path: req.originalUrl,
            statusCode: res.statusCode,
            responseTimeMs: duration,
        }, 'Request processed');
    });

    next();
});

// Serve the built frontend files from `dist/`
app.use(express.static(path.join(__dirname, 'dist')));

// Serve `index.html` for any unknown route (SPA support)
app.get('*', (req, res) => {
    res.sendFile(path.join(__dirname, 'dist', 'index.html'));
});

// Start the Express server
app.listen(PORT, () => {
    logger.info(`Server running on http://localhost:${PORT}`);
});
