package demo.main;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
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
         *
         * @param uuid optional UUID for correlating request and response logs
         * @return a simple message
         */
        @GetMapping("/data")
        public String getData(@RequestHeader(value = "UUID", required = false) String uuid) {
            // Generate UUID if not provided
            if (uuid == null || uuid.isEmpty()) {
                uuid = "generated-" + System.currentTimeMillis();
            }

            // Randomly introduce delays (30% chance of delay)
            if (random.nextDouble() < 0.3) {
                // Random delay between 100ms and 2000ms
                int delayMs = 100 + random.nextInt(1900);
                logger.warn("Simulating slow method - UUID: {}, Delay: {}ms", uuid, delayMs);
                try {
                    Thread.sleep(delayMs);
                } catch (InterruptedException e) {
                    Thread.currentThread().interrupt();
                    logger.error("Sleep interrupted for UUID: {}", uuid);
                }
            }

            String response = "Hello from Spring Boot!";
            logger.info("Spring Response UUID: {}, Response: {}", uuid, response);
            return response;
        }
    }
}
