package demo.main;

import java.io.IOException;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.Response;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.apache.logging.log4j.ThreadContext;

public class GetExample {
    private static final Logger logger = LogManager.getLogger(GetExample.class);
    OkHttpClient client = new OkHttpClient();

    String run(String url) throws IOException {
        Request request = new Request.Builder()
                .url(url)
                .build();

        try (Response response = client.newCall(request).execute()) {
            int statusCode = response.code(); // Get the HTTP response status code
            logger.info(String.format(
                "{\"url\": \"%s\", \"statusCode\": %d, \"trace_id\": \"%s\", \"span_id\": \"%s\", \"trace_flags\": \"%s\"}",
                url,
                statusCode,
                ThreadContext.get("trace_id"),
                ThreadContext.get("span_id"),
                ThreadContext.get("trace_flags")
            ));

            return response.body().string();
        } catch (IOException e) {
            logger.error("An error occurred while making HTTP request to " + url, e);
            throw e; // Re-throwing to maintain method signature and error propagation
        }
    }

    public static void wait(int ms) {
        try {
            Thread.sleep(ms);
        } catch (InterruptedException ex) {
            Thread.currentThread().interrupt();
            logger.error("Thread was interrupted during wait.", ex);
        }
    }

    public static void main(String[] args) {
        String targetURL = System.getProperty("targetURL");
        if (targetURL == null || targetURL.isEmpty()) {
            logger.error("Target URL is not specified. Please provide 'targetURL' as a system property.");
            return;
        }

        while (true) {
            try {
                GetExample example = new GetExample();
                example.run(targetURL); // Log the URL request and the response status code
                wait(300); // Adjust the wait time as necessary
            } catch (IOException e) {
                logger.error("Failed to execute HTTP request.", e);
            }
        }
    }
}
