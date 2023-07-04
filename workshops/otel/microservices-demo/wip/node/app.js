const myHOST = process.env.NODE_TEST_HOST;
const myPORT = process.env.NODE_TEST_PORT;
const myPATH = process.env.NODE_TEST_PATH;

const http = require('http');

function httpget() {
    http.get(options, function(res) {
        console.log("statusCode: ", res.statusCode);
        console.log("headers: ", res.headers);

        let responseData = '';

        res.on('data', function(d) {
            responseData += d;
        });

        res.on('end', function() {
            let parsedData;
            try {
                parsedData = JSON.parse(responseData);
                console.log("Response data:", parsedData);
            } catch (error) {
                console.error("Error parsing JSON:", error);
            }
        });

    }).on('error', function(e) {
        console.error(e);
    });
    console.log("Log");

    // Schedule the next HTTP request
    setTimeout(httpget, interval);
}

const options = {
    hostname: myHOST,
    port: myPORT, // Replace with the appropriate port number
    path: myPATH, // Replace with the appropriate path
};

const interval = 250;

httpget(); // Start the initial HTTP request