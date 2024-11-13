<?php

require 'vendor/autoload.php';

use Symfony\Component\HttpClient\HttpClient;
use Symfony\Contracts\HttpClient\Exception\TransportExceptionInterface;
use Symfony\Contracts\HttpClient\Exception\ClientExceptionInterface;
use Symfony\Contracts\HttpClient\Exception\ServerExceptionInterface;
use Symfony\Contracts\HttpClient\ResponseInterface;

// Function to make a request and log the response
function makeRequestAndLogResponse($url)
{
    $client = HttpClient::create();
    try {
        // Send GET request
        $response = $client->request('GET', $url, ['timeout' => 10]);

        // Log the response
        logResponse($url, $response);
    } catch (ClientExceptionInterface $e) {
        // Handle 4xx client errors
        logError($url, "WARNING", "Client error: " . $e->getMessage(), $e->getResponse());
    } catch (ServerExceptionInterface $e) {
        // Handle 5xx server errors
        logError($url, "ERROR", "Server error: " . $e->getMessage(), $e->getResponse());
    } catch (TransportExceptionInterface $e) {
        // Handle transport errors (e.g., connection issues)
        logError($url, "ERROR", "Transport error: " . $e->getMessage());
    }
}

// Function to log the response with severity
function logResponse($url, ResponseInterface $response)
{
    // Get response status code and body
    $httpCode = $response->getStatusCode();
    $body = $response->getContent(false); // Avoid throwing exceptions on error

    // Determine severity based on HTTP code
    $severity = ($httpCode >= 200 && $httpCode < 300) ? "INFO" :
                (($httpCode >= 400 && $httpCode < 500) ? "WARNING" : "ERROR");

    // Prepare log entry
    $logEntry = [
        'timestamp' => date('c'),
        'url' => $url,
        'status_code' => $httpCode,
        'severity' => $severity,
        'headers' => $response->getHeaders(),
        'body' => json_decode($body, true) ?? $body  // Decode if JSON, else keep as-is
    ];

    // Print log entry to stdout as JSON
    echo json_encode($logEntry) . PHP_EOL;
}

// Function to log errors
function logError($url, $severity, $message, ResponseInterface $response = null)
{
    $logEntry = [
        'timestamp' => date('c'),
        'url' => $url,
        'severity' => $severity,
        'message' => $message,
    ];

    // If there's a response, add status and body to the log entry
    if ($response) {
        $logEntry['status_code'] = $response->getStatusCode();
        $logEntry['body'] = $response->getContent(false); // Avoid exceptions
    }

    echo json_encode($logEntry) . PHP_EOL;
}

// URL to request
$url = "http://cx-php-server:8080/rolldice";

// Loop to make requests every second
while (true) {
    makeRequestAndLogResponse($url);
    sleep(1); // Sleep for 1 second
}
