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
import org.springframework.web.bind.annotation.RequestHeader;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.HashMap;
import java.util.Map;
import java.util.UUID;

@SpringBootApplication
public class MainApplication {
    private final OkHttpClient client = new OkHttpClient();
    private static final ObjectMapper objectMapper = new ObjectMapper();
    private static final SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSSZ");
    private static final Logger logger = LogManager.getLogger(MainApplication.class);

    public static void main(String[] args) {
        SpringApplication.run(MainApplication.class, args);
    }

    @Bean
    public CommandLineRunner runClient() {
        return args -> {
            String targetUrl = "http://localhost:8080/api/data";
            startClientLoop(targetUrl);
        };
    }

    private void startClientLoop(String targetUrl) {
        while (true) {
            String uuid = UUID.randomUUID().toString();
            String okhttpResponse;
            try {
                okhttpResponse = run(targetUrl, uuid);
                log("INFO", targetUrl, okhttpResponse, uuid);
            } catch (IOException e) {
                log("ERROR", targetUrl, e.getMessage(), uuid);
            }
            wait(500);
        }
    }

    private String run(String url, String uuid) throws IOException {
        Request request = new Request.Builder()
                .url(url)
                .addHeader("UUID", uuid)
                .build();
        try (Response response = client.newCall(request).execute()) {
            String responseBody = response.body().string();
            logger.info("OkHttp Request UUID: {}, Request: {}, Response: {}", uuid, request, responseBody);
            return responseBody;
        }
    }

    private static void wait(int ms) {
        try {
            Thread.sleep(ms);
        } catch (InterruptedException ex) {
            Thread.currentThread().interrupt();
        }
    }

    private static void log(String severity, String request, String response, String uuid) {
        Map<String, String> logMap = new HashMap<>();
        logMap.put("timestamp", sdf.format(new Date()));
        logMap.put("severity", severity);
        logMap.put("request", request);
        logMap.put("response", response);
        logMap.put("uuid", uuid);

        try {
            String jsonLog = objectMapper.writeValueAsString(logMap);
            logger.info(jsonLog);
        } catch (IOException e) {
            logger.error("Error creating JSON log", e);
        }
    }

    @RestController
    @RequestMapping("/api")
    public static class ApiController {
        private static final Logger logger = LogManager.getLogger(ApiController.class);

        @GetMapping("/data")
        public String getData(@RequestHeader("UUID") String uuid) {
            String response = "Hello from Spring Boot!";
            logger.info("Spring Response UUID: {}, Response: {}", uuid, response);
            return response;
        }
    }
}
