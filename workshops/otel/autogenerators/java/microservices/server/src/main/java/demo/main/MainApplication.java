package demo.main;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.apache.logging.log4j.ThreadContext;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestHeader;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.Random;

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

        /**
         * Handles GET requests to /api/data and returns a simple message.
         * Randomly introduces delays to simulate slow method execution.
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

                // Randomly introduce delays (30% chance of delay)
                if (random.nextDouble() < 0.3) {
                    int delayMs = 100 + random.nextInt(1900);
                    
                    ThreadContext.put("performance_issue", "true");
                    ThreadContext.put("delay_ms", String.valueOf(delayMs));
                    logger.warn("Simulating slow method execution");
                    
                    try {
                        Thread.sleep(delayMs);
                    } catch (InterruptedException e) {
                        Thread.currentThread().interrupt();
                        ThreadContext.put("error_type", "InterruptedException");
                        logger.error("Sleep interrupted during request processing");
                        return "Request processing interrupted";
                    }
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
            } finally {
                ThreadContext.clearAll();
            }
        }
    }
}
