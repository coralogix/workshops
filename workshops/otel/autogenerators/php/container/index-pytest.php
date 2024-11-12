<?php

use GuzzleHttp\Client;
use GuzzleHttp\Exception\RequestException;

require __DIR__ . '/vendor/autoload.php';

// Set up HTTP client
$client = new Client();
$paymentServiceUrl = 'http://cx-payment-gateway-flask:5000';

// Infinite loop for continuous requests
while (true) {
    // Capture timestamp
    $timestamp = date('Y-m-d H:i:s');
    
    try {
        // Make request to the Flask service
        $serviceResponse = $client->get($paymentServiceUrl);
        $statusCode = $serviceResponse->getStatusCode();
        $bodyContent = $serviceResponse->getBody()->getContents();
    } catch (RequestException $e) {
        // Handle expected errors, such as 404, and capture response if available
        $statusCode = $e->getResponse() ? $e->getResponse()->getStatusCode() : 'N/A';
        $bodyContent = $e->getMessage();
    }

    // Log the result as JSON
    $logEntry = [
        "timestamp" => $timestamp,
        "service_url" => $paymentServiceUrl,
        "status" => $statusCode,
        "response" => $bodyContent,
    ];

    // Output the log in JSON format
    echo json_encode($logEntry) . "\n";

    // Sleep for 1 second before the next request
    sleep(1);
}