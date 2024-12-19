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

import java.util.UUID;

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

        /**
         * Handles GET requests to /api/data and returns a simple message.
         *
         * @param uuid the UUID for correlating request and response logs
         * @return a simple message
         */
        @GetMapping("/data")
        public String getData(@RequestHeader("UUID") String uuid) {
            // Simulating trace and span IDs
            String traceId = UUID.randomUUID().toString();
            String spanId = UUID.randomUUID().toString();

            // Set context variables
            ThreadContext.put("trace_id", traceId); // Use trace_id
            ThreadContext.put("span_id", spanId);   // Use span_id
            ThreadContext.put("uuid", uuid);        // Keep uuid

            String response = "Hello from Spring Boot!";
            logger.info("Spring Response, Response: {}", response);

            // Clear the context after logging
            ThreadContext.clearMap();

            return response;
        }
    }
}
