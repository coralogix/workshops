<?php
// Function to make a cURL request and log the response
function makeRequestAndLogResponse($url)
{
    // Initialize cURL
    $ch = curl_init($url);

    // Set cURL options
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
    curl_setopt($ch, CURLOPT_HEADER, true);
    curl_setopt($ch, CURLOPT_TIMEOUT, 10); // Set timeout for 10 seconds

    // Execute the request
    $response = curl_exec($ch);

    // Capture HTTP status code and other response info
    $httpCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
    $headerSize = curl_getinfo($ch, CURLINFO_HEADER_SIZE);
    $headers = substr($response, 0, $headerSize);
    $body = substr($response, $headerSize);

    // Determine severity based on HTTP code
    $severity = ($httpCode >= 200 && $httpCode < 300) ? "INFO" :
                (($httpCode >= 400 && $httpCode < 500) ? "WARNING" : "ERROR");

    // Log response as JSON
    $logEntry = [
        'timestamp' => date('c'),
        'url' => $url,
        'status_code' => $httpCode,
        'severity' => $severity,
        'headers' => $headers,
        'body' => json_decode($body, true), // Decode body if it's JSON
    ];

    // Write log entry to a file
    file_put_contents('/var/log/cx_php_client.log', json_encode($logEntry) . PHP_EOL, FILE_APPEND);

    // Close the cURL handle
    curl_close($ch);
}

// URL to request
$url = "http://cx-php-server:8080/rolldice";

// Loop to make requests every 1 minute
while (true) {
    makeRequestAndLogResponse($url);
    sleep(1); // Sleep for 1 minute
}
