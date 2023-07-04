package demo.main;

import java.io.IOException;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.Response;
import java.lang.InterruptedException;

public class GetExample {
  OkHttpClient client = new OkHttpClient();

  String run(String url) throws IOException {
    Request request = new Request.Builder()
        .url(url)
        .build();

    try (Response response = client.newCall(request).execute()) {
      return response.body().string();
    }
  }
  
public static void wait(int ms)
{
    try
    {        Thread.sleep(ms);
    }
    catch(InterruptedException ex)
    {        Thread.currentThread().interrupt();
    }
} // wait

public static void main(String[] args) throws IOException {
  String targetUrl = System.getProperty("targetUrl");
  while (true)
    {
       GetExample okhttpexample = new GetExample();
       String okhttpresponse = okhttpexample.run(targetUrl);
       System.out.println(" + targetUrl + " + okhttpresponse);
       wait(100);
      } // while loop
  } // main
} // class