package dk.sdu.mmmi.sempro7E21.grpG.outinthegreen.tracking;

import android.app.Activity;
import android.content.Context;

import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;

public class LocalStorageHelper {
    public void storeNDVI(short ndvi, Activity mainActivity) {
        try {
            FileOutputStream fos = mainActivity.openFileOutput("NDVI", Context.MODE_PRIVATE);
            String row = System.currentTimeMillis() / 1000L + ": "+ndvi+"\n";
            fos.write(row.getBytes());
            fos.close();
        } catch (IOException e) {
            e.printStackTrace();
        }

    }
}
