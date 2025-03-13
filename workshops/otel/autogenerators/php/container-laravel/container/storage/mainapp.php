<?php
use Psr\Http\Message\ResponseInterface as Response;
use Psr\Http\Message\ServerRequestInterface as Request;
use Slim\Factory\AppFactory;
use GuzzleHttp\Client;

require __DIR__ . '/vendor/autoload.php';

$app = AppFactory::create();

// Main app endpoint that rolls a dice and calls the subapp
$app->get('/rolldice', function (Request $request, Response $response) {
    // Automatic instrumentation will create a span for this endpoint

    // Call the subapp
    $client = new Client();
    $subappResponse = $client->get('http://localhost:8081/color');  // Adjust URL if needed
    $color = $subappResponse->getBody()->getContents();

    // Generate a random dice roll result
    $result = random_int(1, 6);
    $response->getBody()->write("Dice: $result, Color: $color");

    return $response;
});

$app->run();
