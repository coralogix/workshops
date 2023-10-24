const request = require('request');

function httpget() {
    const requestOptions = {
        url: 'https://api.github.com',
        headers: {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36'
        }
    };

    request(requestOptions, function(error, response, body) {
        if (error) {
            console.error(error);
            return;
        }

        console.log("statusCode:", response.statusCode);
        console.log("headers:", response.headers);
        console.log(body);
    });

    console.log("This gets logged");
}

const interval = 750;

for (let i = 0; i <= 100000; i++) {
    setTimeout(function(i) {
        httpget();
    }, interval * i, i);
}
