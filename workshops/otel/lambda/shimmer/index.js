const http = require('http');
const { v4: uuidv4 } = require('uuid');
const pino = require('pino');
const shimmer = require('shimmer');
const AWS = require('aws-sdk');

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

// Log that Shimmer is wrapping the http.request function
logger.info({ message: 'Shimmer is wrapping http.request' });

shimmer.wrap(http, 'request', (original) => {
    return function wrappedRequest(options, callback) {
        logger.info({ 
            message: 'Wrapped HTTP request is about to be made', 
            options,
            shimmer: 'before request'
        });

        const req = original.call(this, options, function (res) {
            logger.info({ 
                message: 'Wrapped HTTP request completed', 
                statusCode: res.statusCode,
                shimmer: 'after request'
            });
            
            if (callback) {
                return callback(res);
            }
        });

        req.on('error', (error) => {
            logger.error({ 
                message: 'Error during wrapped HTTP request', 
                error: error.message,
                shimmer: 'error in request'
            });
        });

        return req;
    };
});

// Function to list S3 buckets
async function listS3Buckets() {
    const s3 = new AWS.S3();

    shimmer.wrap(s3, 'listBuckets', (original) => {
        return function wrappedListBuckets(params, callback) {
            logger.info({ message: 'Wrapped S3 listBuckets call is about to be made', shimmer: 'before request' });

            return original.call(this, params, function (err, data) {
                if (err) {
                    logger.error({ message: 'Error during S3 listBuckets call', error: err.message, shimmer: 'error in request' });
                } else {
                    logger.info({ message: 'Wrapped S3 listBuckets call completed', data, shimmer: 'after request' });
                }
                if (callback) {
                    return callback(err, data);
                }
            });
        };
    });

    return new Promise((resolve, reject) => {
        s3.listBuckets((err, data) => {
            if (err) {
                logger.error({ message: 'Error listing S3 buckets', error: err.message });
                reject(err);
            } else {
                logger.info({ message: 'S3 buckets listed successfully', data });
                resolve(data);
            }
        });
    });
}

async function makeRequest() {
    if (requestCount >= 100) {
        logger.info({ 
            timestamp: new Date().toISOString(),
            message: 'Completed 100 requests. Exiting application.'
        });
        return;
    }
    requestCount++; // Increment request counter

    // List S3 buckets before making the Bing request
    await listS3Buckets();

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
