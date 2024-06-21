const http = require('http');
const { v4: uuidv4 } = require('uuid');
const pino = require('pino');

// Determine if running in Lambda or locally
const isLambda = !!process.env.LAMBDA_TASK_ROOT;

// Configure the logger
const logger = isLambda 
    ? pino()
    : pino({
        transport: {
            target: 'pino-pretty',
            options: {
                colorize: true,
                translateTime: true,
                ignore: 'pid,hostname'
            }
        }
    });

let requestCount = 0; // Initialize request counter

async function makeRequest() {
    if (requestCount >= 100) {
        logger.info({ 
            timestamp: new Date().toISOString(),
            message: 'Completed 100 requests. Exiting application.'
        });
        return;
    }
    requestCount++; // Increment request counter

    const requestId = uuidv4();
    const options = {
        hostname: 'www.bing.com',
        port: 80,
        path: '/',
        method: 'GET',
        headers: {
            'X-Request-ID': requestId,
        },
    };

    return new Promise((resolve, reject) => {
        const req = http.request(options, (res) => {
            let data = '';
            res.on('data', (chunk) => {
                data += chunk;
            });
            res.on('end', () => {
                logger.info({
                    timestamp: new Date().toISOString(),
                    requestId,
                    method: options.method,
                    hostname: options.hostname,
                    path: options.path,
                    statusCode: res.statusCode,
                    statusMessage: res.statusMessage,
                    requestCount
                }, 'Request to Bing processed');
                resolve();
            });
        });

        req.on('error', (error) => {
            logger.error({
                timestamp: new Date().toISOString(),
                requestId,
                method: options.method,
                hostname: options.hostname,
                path: options.path,
                error: error.message,
                requestCount
            }, 'Error making request to Bing');
            reject(error);
        });

        req.end();
    });
}

async function handleRequests() {
    try {
        for (let i = 0; i < 100; i++) {
            await makeRequest();
            await new Promise(resolve => setTimeout(resolve, 300)); // Adding a delay between requests
        }
    } catch (error) {
        logger.error({ message: 'Error occurred during requests', error: error.message });
    } finally {
        // Ensure logs are flushed before function exits
        await new Promise(resolve => logger.flush(resolve));
    }
}

// Lambda handler function
exports.handler = async (event) => {
    await handleRequests();
    return {
        statusCode: 200,
        body: JSON.stringify({ message: 'Requests started' }),
    };
};

// If the script is run directly (not as a Lambda function), start making requests
if (require.main === module) {
    handleRequests();
}
