package demo.main;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.apache.logging.log4j.ThreadContext;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.CommandLineRunner;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.annotation.Bean;
import org.springframework.scheduling.annotation.EnableScheduling;
import org.springframework.scheduling.annotation.Scheduled;
import org.springframework.web.reactive.function.client.WebClient;
import org.springframework.web.reactive.function.client.WebClientResponseException;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.time.Duration;
import java.util.UUID;

/**
 * Main application class that uses WebClient to make requests to a target server.
 */
@SpringBootApplication
@EnableScheduling
@RestController
@RequestMapping("/client")
public class MainApplication {
    private static final Logger logger = LogManager.getLogger(MainApplication.class);
    private final WebClient webClient;
    private final String targetUrl;

    @Value("${client.request.timeout:5000}")
    private int requestTimeoutMs;
    
    @Value("${client.loop.interval:500}")
    private int loopIntervalMs;

    public MainApplication(@Value("${target.host:localhost:8080}") String targetHost) {
        this.webClient = WebClient.builder()
            .codecs(configurer -> configurer.defaultCodecs().maxInMemorySize(1024 * 1024))
            .build();
        
        this.targetUrl = "http://" + targetHost + "/api/data";
        
        logger.info("Client initialized with target URL: {}", targetUrl);
    }

    public static void main(String[] args) {
        SpringApplication.run(MainApplication.class, args);
    }

    /**
     * CommandLineRunner to start the WebClient loop after the Spring Boot application has started.
     */
    @Bean
    public CommandLineRunner runClient() {
        return args -> {
            logger.info("Starting background client loop with {}ms interval", loopIntervalMs);
            startClientLoop(this.targetUrl);
        };
    }

    /**
     * Starts the client loop that fetches data from the specified URL.
     */
    private void startClientLoop(String targetUrl) {
        while (!Thread.currentThread().isInterrupted()) {
            String uuid = UUID.randomUUID().toString();
            try {
                makeRequest(targetUrl, uuid);
            } catch (Exception e) {
                // Background requests - minimal logging
            }
            wait(loopIntervalMs);
        }
    }

    /**
     * Makes a GET request using WebClient with timeout and error handling.
     */
    private String makeRequest(String url, String uuid) {
        return webClient.get()
                .uri(url)
                .header("UUID", uuid)
                .retrieve()
                .bodyToMono(String.class)
                .timeout(Duration.ofMillis(requestTimeoutMs))
                .block();
    }

    /**
     * Pauses the current thread for the specified amount of time.
     */
    private static void wait(int ms) {
        try {
            Thread.sleep(ms);
        } catch (InterruptedException ex) {
            Thread.currentThread().interrupt();
            return;
        }
    }

    @GetMapping("/trigger")
    public String triggerRequest() {
        String uuid = UUID.randomUUID().toString();
        
        try {
            ThreadContext.put("event_type", "client_request_started");
            ThreadContext.put("request_uuid", uuid);
            ThreadContext.put("target_url", this.targetUrl);
            logger.info("REST endpoint triggered");
            
            String response = makeRequest(this.targetUrl, uuid);
            
            ThreadContext.put("event_type", "client_request_completed");
            ThreadContext.put("request_uuid", uuid);
            ThreadContext.put("response_body", response);
            ThreadContext.put("status", "success");
            logger.info("REST endpoint completed successfully");
            
            return "Request UUID: " + uuid + ", Response: " + response;
        } catch (WebClientResponseException e) {
            ThreadContext.put("event_type", "client_request_failed");
            ThreadContext.put("request_uuid", uuid);
            ThreadContext.put("error_message", e.getMessage());
            ThreadContext.put("http_status", String.valueOf(e.getRawStatusCode()));
            ThreadContext.put("status", "error");
            logger.error("REST endpoint failed with HTTP error");
            
            return "Request UUID: " + uuid + ", HTTP Error: " + e.getRawStatusCode() + " - " + e.getMessage();
        } catch (Exception e) {
            ThreadContext.put("event_type", "client_request_failed");
            ThreadContext.put("request_uuid", uuid);
            ThreadContext.put("error_message", e.getMessage());
            ThreadContext.put("status", "error");
            logger.error("REST endpoint failed");
            
            return "Request UUID: " + uuid + ", Error: " + e.getMessage();
        } finally {
            ThreadContext.clearAll();
        }
    }

    /**
     * Automatically triggers traced requests every 5 seconds
     */
    @Scheduled(fixedRate = 5000)
    public void automaticTracedRequest() {
        triggerRequest();
    }
}
