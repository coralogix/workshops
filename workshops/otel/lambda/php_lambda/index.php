<?php

use Psr\Http\Message\ResponseInterface as Response;
use Psr\Http\Message\ServerRequestInterface as Request;
use Slim\Factory\AppFactory;

require __DIR__ . '/vendor/autoload.php';

// Check if the ctype extension is loaded
error_log(extension_loaded('ctype') ? "INFO: ctype extension is loaded." : "ERROR: ctype extension is NOT loaded.");

$app = AppFactory::create();

$app->get('/rolldice', function (Request $request, Response $response) {
    $result = random_int(1, 6);

    // Add logs for debugging
    error_log("INFO: Rolled a dice, result: $result");
    echo "DEBUG: Request handled at /rolldice\n";

    $response->getBody()->write(strval($result));
    return $response;
});

// Add a health check route
$app->get('/health', function (Request $request, Response $response) {
    error_log("INFO: Health check invoked."); // stderr
    echo "DEBUG: Health check response sent.\n"; // stdout

    $response->getBody()->write("OK");
    return $response;
});

return $app;
