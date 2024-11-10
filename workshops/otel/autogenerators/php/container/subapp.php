<?php
use Psr\Http\Message\ResponseInterface as Response;
use Psr\Http\Message\ServerRequestInterface as Request;
use Slim\Factory\AppFactory;

require __DIR__ . '/vendor/autoload.php';

$app = AppFactory::create();

// Subapp endpoint to return a random color
$app->get('/color', function (Request $request, Response $response) {
    // Automatic instrumentation will create a span for this endpoint

    // Return a random color
    $colors = ['Red', 'Green', 'Blue', 'Yellow', 'Purple', 'Orange'];
    $color = $colors[array_rand($colors)];
    $response->getBody()->write($color);

    return $response;
});

$app->run();
