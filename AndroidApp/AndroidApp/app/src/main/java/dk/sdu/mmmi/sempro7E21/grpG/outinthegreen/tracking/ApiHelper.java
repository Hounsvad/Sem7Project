package dk.sdu.mmmi.sempro7E21.grpG.outinthegreen.tracking;

import com.google.android.gms.common.api.Api;
import com.google.android.gms.maps.model.VisibleRegion;

import org.json.JSONArray;
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
    private static final String URL = "http://10.0.2.2:8001";
    private static final DecimalFormat df = new DecimalFormat("0.000000");
    private static ApiHelper instance;
    private final OkHttpClient client;

    private ApiHelper() {
        this.client = new OkHttpClient().newBuilder().build();
    }

    public static ApiHelper getInstance() {
        if (instance == null){
            instance = new ApiHelper();
        }
        return instance;
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
                .url(URL+"/ndvi")
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

    public double[][][] getOverlayCoordinates(VisibleRegion visibleRegion){
        try {
            MediaType mediaType = MediaType.parse("application/json");
            String json = new JSONObject()
                    .put("latNearLeft", df.format(visibleRegion.nearLeft.latitude).replaceAll("\\.",""))
                    .put("longNearLeft", df.format(visibleRegion.nearLeft.longitude).replaceAll("\\.",""))
                    .put("latFarRight", df.format(visibleRegion.farRight.latitude).replaceAll("\\.",""))
                    .put("longFarRight", df.format(visibleRegion.farRight.longitude).replaceAll("\\.",""))
                    .toString();
            RequestBody body = RequestBody.create(json, mediaType);
            Request request = new Request.Builder()
                    .url(URL+"/coordinates")
                    .method("POST", body)
                    .addHeader("Content-Type", "application/json")
                    .build();
            Response response = client.newCall(request).execute();

            JSONArray cells = new JSONObject(response.body().string()).getJSONArray("cells");
            double[][][] array = new double[cells.length()][4][2];

            for (int i = 0; i < cells.length(); i++){
                JSONArray cellCoordinates = cells.getJSONArray(i);
                for (int j = 0; j < 4; j++){
                    JSONArray cellCoordinatesParts = cellCoordinates.getJSONArray(j);
                    array[i][j][0] = cellCoordinatesParts.getDouble(0);
                    array[i][j][1] = cellCoordinatesParts.getDouble(1);
                }
            }

            return array;
        } catch (IOException | JSONException e) {
            e.printStackTrace();
            return new double[][][]{{{}}};
        }
    }
}
