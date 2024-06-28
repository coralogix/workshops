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
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.HashMap;
import java.util.Map;

/**
 * Main application class that starts the Spring Boot server and the OkHttp client.
 */
@SpringBootApplication
public class MainApplication {
    private final OkHttpClient client = new OkHttpClient();
    private static final ObjectMapper objectMapper = new ObjectMapper();
    private static final SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSSZ");
    private static final Logger logger = LogManager.getLogger(MainApplication.class);

    public static void main(String[] args) {
        // Start the Spring Boot application
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
            String targetUrl = "http://localhost:8080/api/data";
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
            String okhttpResponse;
            try {
                okhttpResponse = run(targetUrl);
                log("INFO", targetUrl, okhttpResponse);
            } catch (IOException e) {
                log("ERROR", targetUrl, e.getMessage());
            }

            wait(500);
        }
    }

    /**
     * Makes a GET request to the specified URL and returns the response as a string.
     *
     * @param url the URL to fetch data from
     * @return the response from the URL as a string
     * @throws IOException if an I/O error occurs
     */
    private String run(String url) throws IOException {
        Request request = new Request.Builder().url(url).build();
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

    /**
     * Logs the request and response data.
     *
     * @param severity the severity of the log
     * @param request  the request URL
     * @param response the response data
     */
    private static void log(String severity, String request, String response) {
        Map<String, String> logMap = new HashMap<>();
        logMap.put("timestamp", sdf.format(new Date()));
        logMap.put("severity", severity);
        logMap.put("request", request);
        logMap.put("response", response);

        try {
            String jsonLog = objectMapper.writeValueAsString(logMap);
            logger.info(jsonLog);
        } catch (IOException e) {
            logger.error("Error creating JSON log", e);
        }
    }

    /**
     * Spring Boot REST controller that provides the /api/data endpoint.
     */
    @RestController
    @RequestMapping("/api")
    public static class ApiController {

        /**
         * Handles GET requests to /api/data and returns a simple message.
         *
         * @return a simple message
         */
        @GetMapping("/data")
        public String getData() {
            return "Hello from Spring Boot!";
        }
    }
}
