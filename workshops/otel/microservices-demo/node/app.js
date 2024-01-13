const http = require('http');

/**
 * Function to make an HTTP GET request to the GitHub API.
 */
function httpget() {
    const options = {
        hostname: 'api.github.com',
        path: '/',
        method: 'GET',
        headers: {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36'
        }
    };

    const req = http.request(options, (res) => {
        console.log(`statusCode: ${res.statusCode}`);
        console.log(`headers: ${JSON.stringify(res.headers)}`);

        res.on('data', (d) => {
            process.stdout.write(d);
        });
    });

    req.on('error', (error) => {
        console.error(error);
    });

    req.end();
}

console.log("This gets logged");

/**
 * Function to generate a random interval between 500ms and 1000ms.
 * @returns {number} - Random interval in milliseconds.
 */
function getRandomInterval() {
    return Math.floor(Math.random() * (1000 - 500 + 1) + 500);
}

/**
 * Function to perform the HTTP GET requests with randomized intervals.
 */
function performHttpRequests() {
    const interval = getRandomInterval();
    httpget();

    // Set a new timeout for the next HTTP GET request.
    setTimeout(performHttpRequests, interval);
}

// Start the loop for making HTTP GET requests.
performHttpRequests();
