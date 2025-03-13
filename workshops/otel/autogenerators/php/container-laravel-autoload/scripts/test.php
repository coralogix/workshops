<?php

require __DIR__ . '/../vendor/autoload.php'; // Load Composer's autoloader

use Illuminate\Support\Facades\Route;
use Illuminate\Http\Request;

// Manual route simulation for standalone testing
echo "Autoload test: vendor/autoload.php loaded successfully!\n";

// Simulate a Laravel-like request
$testRequest = Request::create('/rolldice', 'GET');

// Define a test route
Route::get('/rolldice', function () {
    return response()->json(['roll' => random_int(1, 6)]);
});

// Dispatch the request
$response = Route::dispatch($testRequest);

// Output response
echo "Route test response: " . $response->getContent() . "\n";
