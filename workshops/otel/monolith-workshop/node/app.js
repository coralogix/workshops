const http = require('http');

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

const interval = 750;

for (let i = 0; i <= 1000; i++) {
    setTimeout(function(i) {
        httpget();
    }, interval * i, i);
}