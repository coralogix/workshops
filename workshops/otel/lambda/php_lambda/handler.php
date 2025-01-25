<?php
require "/var/task/vendor/autoload.php";
require "/var/task/index.php";

// Decode the Lambda event
$event = json_decode(file_get_contents("php://stdin"), true);

// Map the Lambda event to Slim's HTTP environment
$_SERVER["REQUEST_METHOD"] = $event["httpMethod"] ?? "GET";
$_SERVER["REQUEST_URI"] = $event["path"] ?? "/";
$_SERVER["QUERY_STRING"] = http_build_query($event["queryStringParameters"] ?? []);
$_SERVER["CONTENT_TYPE"] = $event["headers"]["Content-Type"] ?? "application/json";

foreach ($event["headers"] ?? [] as $key => $value) {
    $headerKey = "HTTP_" . strtoupper(str_replace("-", "_", $key));
    $_SERVER[$headerKey] = $value;
}

// Use output buffering to capture Slim's response
ob_start();
$app = require "/var/task/index.php";
$app->run();
$response = ob_get_clean();

// Format the response for Lambda
echo json_encode([
    "statusCode" => 200,
    "headers" => ["Content-Type" => "application/json"],
    "body" => $response
]);
