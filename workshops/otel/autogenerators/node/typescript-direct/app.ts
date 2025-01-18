import express, { Express } from 'express';
import http from 'http'; // Import the HTTP module
import pino from 'pino'; // Logger library

const PORT: number = parseInt(process.env.PORT || '8080');
const app: Express = express();
const logger = pino(); // Initialize Pino logger

let requestCount = 0; // Track the number of requests made

// Helper function to generate a random number
function getRandomNumber(min: number, max: number): number {
  return Math.floor(Math.random() * (max - min + 1) + min);
}

// Express route to handle requests
app.get('/rolldice', (req, res) => {
  const roll = getRandomNumber(1, 6);
  const responseBody = { roll };

  res.status(200).json(responseBody);

  logger.info({
    timestamp: new Date().toISOString(),
    method: req.method,
    path: req.url,
    statusCode: res.statusCode,
    responseBody,
  }, 'Request processed');
});

// Function to start the Express server
function startServer() {
  app.listen(PORT, () => {
    logger.info(`Server is running on http://localhost:${PORT}`);
    makeRequestsToSelf(); // Start making requests to itself
  });
}

// HTTP client function to make requests to itself
function makeRequestsToSelf() {
  const options = {
    hostname: 'localhost',
    port: PORT,
    path: '/rolldice',
    method: 'GET',
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
          method: 'GET',
          path: options.path,
          statusCode: res.statusCode,
          responseBody,
        }, 'Request received and processed');
      } catch (err) {
        if (err instanceof Error) {
          logger.error({ error: err.message }, 'Error parsing response');
        } else {
          logger.error({ error: 'An unknown error occurred' }, 'Error parsing response');
        }
      }
      // Schedule the next request with a random delay
      const delay = getRandomNumber(250, 1500); // Delay between 250ms and 1500ms
      setTimeout(makeRequestsToSelf, delay);
    });
  });

  // Handle request errors
  req.on('error', (error) => {
    logger.error({ error: error.message }, 'Request failed');
  });

  // Handle request timeout
  req.setTimeout(5000, () => {
    logger.warn('Request timed out');
    req.destroy();
  });

  req.end();
}

// Start the server
startServer();
