const request = require('request');

function httpget() {
    request(options, function(error, response, body) {
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

const options = {
    url: 'https://api.github.com',
};

const interval = 750;

for (let i = 0; i <= 250; i++) {
    setTimeout(function(i) {
        httpget();
    }, interval * i, i);
}