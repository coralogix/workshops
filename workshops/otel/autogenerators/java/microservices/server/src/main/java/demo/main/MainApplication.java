package demo.main;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.github.benmanes.caffeine.cache.Cache;
import com.github.benmanes.caffeine.cache.Caffeine;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.Response;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.apache.logging.log4j.ThreadContext;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestHeader;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.io.IOException;
import java.math.BigInteger;
import java.security.MessageDigest;
import java.time.Duration;
import java.util.*;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.ThreadLocalRandom;

/**
 * Main application class that starts the Spring Boot server.
 */
@SpringBootApplication
public class MainApplication {

    public static void main(String[] args) {
        SpringApplication.run(MainApplication.class, args);
    }

    /**
     * Spring Boot REST controller that provides the /api/data endpoint.
     */
    @RestController
    @RequestMapping("/api")
    public static class ApiController {
        private static final Logger logger = LogManager.getLogger(ApiController.class);
        private static final Random random = new Random();
        
        private final OkHttpClient httpClient = new OkHttpClient.Builder()
                .connectTimeout(Duration.ofSeconds(10))
                .readTimeout(Duration.ofSeconds(10))
                .build();
        
        private final ObjectMapper objectMapper = new ObjectMapper();
        
        private final Cache<String, String> cache = Caffeine.newBuilder()
                .maximumSize(1000)
                .expireAfterWrite(Duration.ofMinutes(5))
                .build();

        /**
         * Handles GET requests to /api/data and returns a simple message.
         * Randomly introduces realistic performance bottlenecks.
         */
        @GetMapping("/data")
        public String getData(@RequestHeader(value = "UUID", required = false) String uuid) {
            // Generate UUID if not provided
            if (uuid == null || uuid.isEmpty()) {
                uuid = "generated-" + System.currentTimeMillis();
            }

            try {
                ThreadContext.put("event_type", "server_request_received");
                ThreadContext.put("request_uuid", uuid);
                ThreadContext.put("endpoint", "/api/data");
                ThreadContext.put("method", "GET");

                // Randomly introduce realistic performance issues (30% chance)
                if (random.nextDouble() < 0.3) {
                    ThreadContext.put("performance_issue", "true");
                    logger.warn("Simulating realistic performance bottleneck");
                    
                    // Randomly pick a realistic slow operation
                    executeRandomSlowOperation(uuid);
                } else {
                    ThreadContext.put("performance_issue", "false");
                }

                String response = "Hello from Spring Boot! UUID: " + uuid;
                ThreadContext.put("event_type", "server_response_sent");
                ThreadContext.put("request_uuid", uuid);
                ThreadContext.put("response_body", response);
                ThreadContext.put("status", "success");
                logger.info("Request processed successfully");
                return response;
            } catch (Exception e) {
                ThreadContext.put("error_type", e.getClass().getSimpleName());
                ThreadContext.put("error_message", e.getMessage());
                logger.error("Error during request processing", e);
                return "Error processing request: " + e.getMessage();
            } finally {
                ThreadContext.clearAll();
            }
        }

        /**
         * Randomly executes one of several realistic slow operations using OpenTelemetry-compatible frameworks
         */
        private void executeRandomSlowOperation(String uuid) {
            String[] operations = {"http_client", "json_processing", "cache_loading", "cpu_intensive", "concurrent_processing"};
            String operation = operations[random.nextInt(operations.length)];
            
            ThreadContext.put("slow_operation_type", operation);
            long startTime = System.currentTimeMillis();
            
            try {
                switch (operation) {
                    case "http_client":
                        performSlowHttpClientOperation(uuid);
                        break;
                    case "json_processing":
                        performHeavyJsonProcessing(uuid);
                        break;
                    case "cache_loading":
                        performSlowCacheOperation(uuid);
                        break;
                    case "cpu_intensive":
                        performCpuIntensiveOperation(uuid);
                        break;
                    case "concurrent_processing":
                        performConcurrentProcessing(uuid);
                        break;
                }
            } finally {
                long duration = System.currentTimeMillis() - startTime;
                ThreadContext.put("operation_duration_ms", String.valueOf(duration));
                logger.warn("Completed slow operation: {} in {}ms", operation, duration);
            }
        }

        /**
         * Simulates slow HTTP client calls using OkHttp (auto-instrumented by OpenTelemetry)
         */
        private void performSlowHttpClientOperation(String uuid) {
            try {
                // Make call to a slow endpoint (httpbin.org delay endpoint)
                Request request = new Request.Builder()
                        .url("https://httpbin.org/delay/" + (1 + random.nextInt(3)))
                        .addHeader("X-Request-UUID", uuid)
                        .build();
                
                try (Response response = httpClient.newCall(request).execute()) {
                    String responseBody = response.body() != null ? response.body().string() : "";
                    ThreadContext.put("http_response_size", String.valueOf(responseBody.length()));
                }
            } catch (IOException e) {
                logger.warn("HTTP client operation failed: {}", e.getMessage());
                // Fallback to local computation if external call fails
                performCpuIntensiveOperation(uuid);
            }
        }

        /**
         * Simulates heavy JSON processing using Jackson (can be traced by OpenTelemetry)
         */
        private void performHeavyJsonProcessing(String uuid) {
            try {
                // Create a large, complex object
                Map<String, Object> complexData = new HashMap<>();
                complexData.put("uuid", uuid);
                complexData.put("timestamp", System.currentTimeMillis());
                
                // Generate large nested structure
                List<Map<String, Object>> largeList = new ArrayList<>();
                for (int i = 0; i < 2000; i++) {
                    Map<String, Object> item = new HashMap<>();
                    item.put("id", i);
                    item.put("data", "Large data string " + i + " " + UUID.randomUUID().toString());
                    item.put("nested", Collections.singletonMap("value", "Nested value " + i));
                    
                    // Add some complex nested structures
                    Map<String, Object> deepNested = new HashMap<>();
                    for (int j = 0; j < 10; j++) {
                        deepNested.put("field_" + j, "Deep nested data " + j + " for item " + i);
                    }
                    item.put("deep_nested", deepNested);
                    largeList.add(item);
                }
                complexData.put("large_list", largeList);
                
                // Serialize and deserialize multiple times
                for (int i = 0; i < 10; i++) {
                    String json = objectMapper.writeValueAsString(complexData);
                    Map<String, Object> parsed = objectMapper.readValue(json, Map.class);
                    ThreadContext.put("json_size_bytes", String.valueOf(json.length()));
                    
                    // Add some processing of the parsed data
                    processJsonData(parsed);
                }
            } catch (Exception e) {
                logger.warn("JSON processing operation failed: {}", e.getMessage());
            }
        }

        private void processJsonData(Map<String, Object> data) {
            // Simulate processing the parsed JSON data
            if (data.containsKey("large_list")) {
                List<Map<String, Object>> list = (List<Map<String, Object>>) data.get("large_list");
                int processedItems = 0;
                for (Map<String, Object> item : list) {
                    if (item.containsKey("data")) {
                        String itemData = (String) item.get("data");
                        // Simulate some string processing
                        itemData.toUpperCase().toLowerCase().trim();
                        processedItems++;
                    }
                }
                ThreadContext.put("processed_json_items", String.valueOf(processedItems));
            }
        }

        /**
         * Simulates slow cache loading operations using Caffeine (supported by OpenTelemetry)
         */
        private void performSlowCacheOperation(String uuid) {
            // Simulate cache miss with expensive computation
            String cacheKey = "expensive-computation-" + (random.nextInt(20));
            
            String result = cache.get(cacheKey, key -> {
                // Simulate expensive computation that would warrant caching
                StringBuilder largeString = new StringBuilder();
                for (int i = 0; i < 50000; i++) {
                    largeString.append("Expensive computation for ")
                            .append(key)
                            .append(" iteration ")
                            .append(i)
                            .append(" with UUID ")
                            .append(uuid)
                            .append(" timestamp ")
                            .append(System.currentTimeMillis())
                            .append("\n");
                }
                
                // Add some CPU-intensive processing
                String content = largeString.toString();
                try {
                    MessageDigest md = MessageDigest.getInstance("SHA-256");
                    byte[] hash = md.digest(content.getBytes());
                    return content + " Hash: " + new BigInteger(1, hash).toString(16);
                } catch (Exception e) {
                    return content;
                }
            });
            
            ThreadContext.put("cache_result_size", String.valueOf(result.length()));
        }

        /**
         * Simulates CPU-intensive operations that naturally take time
         */
        private void performCpuIntensiveOperation(String uuid) {
            try {
                MessageDigest md = MessageDigest.getInstance("SHA-256");
                String baseString = uuid + System.currentTimeMillis();
                
                // Perform computationally expensive operations
                for (int i = 0; i < 5000; i++) {
                    String input = baseString + i;
                    byte[] hash = md.digest(input.getBytes());
                    
                    // Convert to BigInteger for additional computation
                    BigInteger number = new BigInteger(1, hash);
                    
                    // Perform some mathematical operations
                    BigInteger result = number.multiply(BigInteger.valueOf(i + 1))
                                             .mod(BigInteger.valueOf(982451653L));
                    
                    // Add some string manipulations
                    String hexResult = result.toString(16);
                    hexResult.toUpperCase().toLowerCase().replace("a", "x");
                    
                    if (i % 500 == 0) {
                        ThreadContext.put("computation_progress", String.valueOf(i));
                    }
                }
                
                // Simulate some prime number computation
                calculatePrimesUpTo(1000);
                
            } catch (Exception e) {
                logger.warn("CPU intensive operation failed: {}", e.getMessage());
            }
        }

        /**
         * Simulates concurrent processing operations
         */
        private void performConcurrentProcessing(String uuid) {
            try {
                List<CompletableFuture<String>> futures = new ArrayList<>();
                
                // Create multiple concurrent tasks
                for (int i = 0; i < 10; i++) {
                    final int taskId = i;
                    CompletableFuture<String> future = CompletableFuture.supplyAsync(() -> {
                        // Each task does some work
                        StringBuilder result = new StringBuilder();
                        for (int j = 0; j < 1000; j++) {
                            result.append("Task ").append(taskId)
                                  .append(" iteration ").append(j)
                                  .append(" for UUID ").append(uuid)
                                  .append("\n");
                        }
                        
                        // Add some computation per task
                        try {
                            MessageDigest md = MessageDigest.getInstance("MD5");
                            byte[] hash = md.digest(result.toString().getBytes());
                            return "Task " + taskId + " completed with hash: " + 
                                   new BigInteger(1, hash).toString(16);
                        } catch (Exception e) {
                            return "Task " + taskId + " completed";
                        }
                    });
                    futures.add(future);
                }
                
                // Wait for all tasks to complete
                CompletableFuture.allOf(futures.toArray(new CompletableFuture[0])).join();
                
                int completedTasks = futures.size();
                ThreadContext.put("concurrent_tasks_completed", String.valueOf(completedTasks));
                
            } catch (Exception e) {
                logger.warn("Concurrent processing operation failed: {}", e.getMessage());
            }
        }

        private void calculatePrimesUpTo(int limit) {
            List<Integer> primes = new ArrayList<>();
            for (int num = 2; num <= limit; num++) {
                boolean isPrime = true;
                for (int i = 2; i * i <= num; i++) {
                    if (num % i == 0) {
                        isPrime = false;
                        break;
                    }
                }
                if (isPrime) {
                    primes.add(num);
                }
            }
            ThreadContext.put("primes_calculated", String.valueOf(primes.size()));
        }
    }
}
