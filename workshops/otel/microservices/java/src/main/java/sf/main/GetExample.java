package demo.main;

import com.fasterxml.jackson.databind.ObjectMapper;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.Response;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;

import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.HashMap;
import java.util.Map;

public class GetExample {
    private final OkHttpClient client = new OkHttpClient();
    private static final ObjectMapper objectMapper = new ObjectMapper();
    private static final SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSSZ");
    private static final Logger logger = LogManager.getLogger(GetExample.class);

    public String run(String url) throws IOException {
        Request request = new Request.Builder().url(url).build();

        try (Response response = client.newCall(request).execute()) {
            return response.body().string();
        }
    }

    public static void wait(int ms) {
        try {
            Thread.sleep(ms);
        } catch (InterruptedException ex) {
            Thread.currentThread().interrupt();
        }
    }

    public static void log(String severity, String request, String response) {
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

    public static void main(String[] args) throws IOException {
        String targetUrl = System.getProperty("targetUrl");

        GetExample okHttpExample = new GetExample();

        while (true) {
            String okhttpResponse;
            try {
                okhttpResponse = okHttpExample.run(targetUrl);
                log("INFO", targetUrl, okhttpResponse);
            } catch (IOException e) {
                log("ERROR", targetUrl, e.getMessage());
            }

            wait(500);
        }
    }
}
