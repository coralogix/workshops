const mysql = require('mysql');
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

const RDS_HOST = process.env.RDS_HOST;
const RDS_USER = process.env.RDS_USER;
const RDS_PASSWORD = process.env.RDS_PASSWORD;
const RDS_DATABASE = process.env.RDS_DATABASE;

const handler = async (event) => {
  const connection = mysql.createConnection({
    host: RDS_HOST,
    user: RDS_USER,
    password: RDS_PASSWORD,
    database: RDS_DATABASE,
  });

  const requestId = uuidv4();
  const timestamp = new Date().toISOString();

  try {
    await connectToDatabase(connection, requestId);

    // Ensure the table exists
    await ensureTableExists(connection, requestId);

    // Insert random values
    await populateDatabase(connection, requestId);

    // Erase the database
    const erasedResults = await eraseDatabase(connection, requestId);

    connection.end();

    return {
      statusCode: 200,
      body: JSON.stringify('Database populated and erased successfully.'),
    };
  } catch (error) {
    logger.error({
      timestamp,
      requestId,
      severity: 'ERROR',
      action: 'error',
      error: error.message,
      message: 'Error occurred during database operation'
    });

    connection.end();

    return {
      statusCode: 500,
      body: JSON.stringify(`Error: ${error.message}`),
    };
  } finally {
    // Ensure logs are flushed before function exits
    await new Promise(resolve => logger.flush(resolve));
  }
};

const connectToDatabase = (connection, requestId) => {
  return new Promise((resolve, reject) => {
    const timestamp = new Date().toISOString();
    connection.connect((err) => {
      if (err) {
        logger.error({
          timestamp,
          requestId,
          severity: 'ERROR',
          sql: 'CONNECT',
          error: err.message
        });
        return reject(err);
      }
      logger.info({
        timestamp,
        requestId,
        severity: 'INFO',
        sql: 'CONNECT',
        message: 'Connected to database'
      });
      resolve();
    });
  });
};

const ensureTableExists = (connection, requestId) => {
  return new Promise((resolve, reject) => {
    const query = `
      CREATE TABLE IF NOT EXISTS your_table (
        id INT AUTO_INCREMENT PRIMARY KEY,
        column1 VARCHAR(255),
        column2 INT
      )
    `;
    const timestamp = new Date().toISOString();
    connection.query(query, (err, results) => {
      if (err) {
        logger.error({
          timestamp,
          requestId,
          severity: 'ERROR',
          sql: query,
          error: err.message
        });
        return reject(err);
      }
      logger.info({
        timestamp,
        requestId,
        severity: 'INFO',
        sql: query,
        result: results,
        message: 'Ensured the table exists'
      });
      resolve(results);
    });
  });
};

const populateDatabase = async (connection, requestId) => {
  const query = 'INSERT INTO your_table (column1, column2) VALUES (?, ?)';
  const promises = [];
  const values = [];

  // Generating random values
  for (let i = 0; i < 100; i++) {
    values.push([Math.random().toString(36).substring(7), Math.floor(Math.random() * 100)]);
  }

  for (const value of values) {
    promises.push(new Promise((resolve, reject) => {
      const timestamp = new Date().toISOString();
      connection.query(query, value, (err, results) => {
        if (err) {
          logger.error({
            timestamp,
            requestId,
            severity: 'ERROR',
            sql: query,
            values: value,
            error: err.message
          });
          return reject(err);
        }
        logger.info({
          timestamp,
          requestId,
          severity: 'INFO',
          sql: query,
          values: value,
          result: results,
          message: 'Inserted row into database'
        });
        resolve(results);
      });
    }));
  }

  return Promise.all(promises);
};

const eraseDatabase = (connection, requestId) => {
  return new Promise((resolve, reject) => {
    const query = 'DELETE FROM your_table';
    const timestamp = new Date().toISOString();
    connection.query(query, (err, results) => {
      if (err) {
        logger.error({
          timestamp,
          requestId,
          severity: 'ERROR',
          sql: query,
          error: err.message
        });
        return reject(err);
      }
      logger.info({
        timestamp,
        requestId,
        severity: 'INFO',
        sql: query,
        result: results,
        message: 'Database erased successfully'
      });
      resolve(results);
    });
  });
};

// Export the handler function for AWS Lambda
exports.handler = handler;

// Run the function if the script is executed directly (for local testing)
if (require.main === module) {
  // Simulate an empty event object
  handler({}).then((response) => {
    console.log(response);
  }).catch((error) => {
    console.error(error);
  });
}
