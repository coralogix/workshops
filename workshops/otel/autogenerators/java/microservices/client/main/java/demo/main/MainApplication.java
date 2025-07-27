package demo.main;

import com.fasterxml.jackson.databind.ObjectMapper;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.Response;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.boot.CommandLineRunner;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.annotation.Bean;

import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.HashMap;
import java.util.Map;
import java.util.UUID;

/**
 * Main application class that starts the OkHttp client to make requests to a target server.
 */
@SpringBootApplication
public class MainApplication {
    private final OkHttpClient client = new OkHttpClient();
    private static final ObjectMapper objectMapper = new ObjectMapper();
    private static final SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSSZ");
    private static final Logger logger = LogManager.getLogger(MainApplication.class);

    public static void main(String[] args) {
        SpringApplication.run(MainApplication.class, args);
    }

    /**
     * CommandLineRunner to start the OkHttp client loop after the Spring Boot application has started.
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
                String response = run(targetUrl, uuid);
                logger.info("Client Request UUID: {}, Response: {}", uuid, response);
            } catch (IOException e) {
                logger.error("OkHttp Error UUID: {}, Error: {}", uuid, e.getMessage());
            }
            wait(500);
        }
    }

    /**
     * Makes a GET request to the specified URL and returns the response as a string.
     *
     * @param url  the URL to fetch data from
     * @param uuid the UUID for correlating request and response logs
     * @return the response from the URL as a string
     * @throws IOException if an I/O error occurs
     */
    private String run(String url, String uuid) throws IOException {
        Request request = new Request.Builder()
                .url(url)
                .addHeader("UUID", uuid)
                .build();
        try (Response response = client.newCall(request).execute()) {
            return response.body().string();
        }
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
