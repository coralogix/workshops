package demo.main;

import com.fasterxml.jackson.databind.ObjectMapper;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.apache.logging.log4j.ThreadContext;
import org.springframework.boot.CommandLineRunner;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.annotation.Bean;
import org.springframework.web.reactive.function.client.WebClient;

import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.HashMap;
import java.util.Map;
import java.util.UUID;

/**
 * Main application class that demonstrates modern HTTP client usage with WebClient.
 */
@SpringBootApplication
public class MainApplication {
    private static final ObjectMapper objectMapper = new ObjectMapper();
    private static final SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSSZ");
    private static final Logger logger = LogManager.getLogger(MainApplication.class);
    private final WebClient webClient;

    public MainApplication() {
        this.webClient = WebClient.builder()
            .codecs(configurer -> configurer.defaultCodecs().maxInMemorySize(1024 * 1024))
            .build();
    }

    public static void main(String[] args) {
        SpringApplication.run(MainApplication.class, args);
    }

    /**
     * CommandLineRunner to start the WebClient loop after the Spring Boot application has started.
     *
     * @return the CommandLineRunner bean
     */
    @Bean
    public CommandLineRunner runClient() {
        return args -> {
            // Get target URL from environment variable, fallback to localhost for local development
            String targetHost = System.getenv("TARGET_HOST");
            if (targetHost == null || targetHost.isEmpty()) {
                targetHost = "localhost:8080";
                logger.warn("TARGET_HOST environment variable not set, using default: {}", targetHost);
            }
            
            String targetUrl = "http://" + targetHost + "/api/data";
            logger.info("Client starting, target URL: {}", targetUrl);
            startClientLoop(targetUrl);
        };
    }

    /**
     * Starts the client loop that fetches data from the specified URL every 500 milliseconds.
     *
     * @param targetUrl the URL to fetch data from
     */
    private void startClientLoop(String targetUrl) {
        while (true) {
            String uuid = UUID.randomUUID().toString();
            try {
                // Add structured data to ThreadContext for JSON logging
                ThreadContext.put("client_request_uuid", uuid);
                ThreadContext.put("target_url", targetUrl);
                
                String response = makeRequest(targetUrl, uuid);
                
                // Add response data to context
                ThreadContext.put("response_content", response);
                ThreadContext.put("status", "success");
                
                logger.info("Client request completed successfully");
                
            } catch (Exception e) {
                // Add error data to context
                ThreadContext.put("client_request_uuid", uuid);
                ThreadContext.put("target_url", targetUrl);
                ThreadContext.put("status", "error");
                ThreadContext.put("error_message", e.getMessage());
                
                logger.error("WebClient request failed");
            } finally {
                // Clear context to prevent memory leaks
                ThreadContext.clearAll();
            }
            wait(500);
        }
    }

    /**
     * Makes a GET request using WebClient to the specified URL and returns the response as a string.
     *
     * @param url  the URL to fetch data from
     * @param uuid the UUID for correlating request and response logs
     * @return the response from the URL as a string
     */
    private String makeRequest(String url, String uuid) {
        return webClient.get()
                .uri(url)
                .header("UUID", uuid)
                .retrieve()
                .bodyToMono(String.class)
                .block(); // Convert to synchronous for this example
    }

    /**
     * Pauses the current thread for the specified amount of time.
     *
     * @param ms the amount of time to wait in milliseconds
     */
    private static void wait(int ms) {
        try {
            Thread.sleep(ms);
        } catch (InterruptedException ex) {
            Thread.currentThread().interrupt();
        }
    }
}
