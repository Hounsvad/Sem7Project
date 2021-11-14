package dk.sdu.mmmi.sempro7E21.grpG.outinthegreen.tracking;

import org.json.JSONException;
import org.json.JSONObject;

import java.io.IOException;
import java.text.DecimalFormat;

import okhttp3.MediaType;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.RequestBody;
import okhttp3.Response;

public class ApiHelper {

    private static final DecimalFormat df = new DecimalFormat("0.000000");

    private final OkHttpClient client;

    public ApiHelper() {
        this.client = new OkHttpClient().newBuilder().build();
    }

    public short getNDVI(double latitude, double longitude) {

        try {
            MediaType mediaType = MediaType.parse("application/json");
            String json = new JSONObject()
                    .put("latitude", df.format(latitude).replaceAll("\\.",""))
                    .put("longitude", df.format(longitude).replaceAll("\\.",""))
                    .toString();
            RequestBody body = RequestBody.create(json, mediaType);
            Request request = new Request.Builder()
                .url("http://10.0.2.2:8001/ndvi")
                .method("POST", body)
                .addHeader("Content-Type", "application/json")
                .build();
            Response response = client.newCall(request).execute();
            return Short.parseShort(response.body().string());
        } catch (IOException | JSONException e) {
            e.printStackTrace();
            return -100;
        }
    }
}
