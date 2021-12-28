package dk.sdu.mmmi.sempro7E21.grpG.outinthegreen.tracking;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.Context;

import java.io.BufferedReader;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.text.SimpleDateFormat;
import java.time.Instant;
import java.util.Date;
import java.util.HashMap;
import java.util.Map;

public class LocalStorageHelper {
    private static final double THRESHOLD = 0.5;

    public static void storeNDVI(double ndvi, Activity mainActivity) {
        if (ndvi > 100 || ndvi < 0){
            return;
        }
        try {
            @SuppressLint("SimpleDateFormat") FileOutputStream fos = mainActivity.openFileOutput("NDVI_"+ new SimpleDateFormat("yyyy-MM-dd").format(new Date()), Context.MODE_PRIVATE);
            String row = System.currentTimeMillis() / 1000L + ": "+ndvi+"\n";
            fos.write(row.getBytes());
            fos.close();
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    public static void storeDailySeconds(int seconds, Activity mainActivity){
        try {
            FileOutputStream fos = mainActivity.openFileOutput("Progress_Weekly", Context.MODE_PRIVATE);
            @SuppressLint("SimpleDateFormat") String row = new SimpleDateFormat("yyyy-MM-dd").format(new Date()) + ": "+seconds+"\n";
            fos.write(row.getBytes());
            fos.close();
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    public static int calculateSecondsSpentInGreenToday(Activity mainActivity) {
        int timeSpent = 0;
        try {
            long lastTimestamp = -1L;
            @SuppressLint("SimpleDateFormat") BufferedReader reader  = new BufferedReader(new InputStreamReader(mainActivity.openFileInput("NDVI_"+ new SimpleDateFormat("yyyy-MM-dd").format(new Date()))));
            while(reader.ready()) {
                String[] line = reader.readLine().split(": ");
                String time = line[0];
                double ndvi = Double.parseDouble(line[1]);

                if (lastTimestamp != -1L){
                    timeSpent += Long.parseLong(time) - lastTimestamp;
                }
                lastTimestamp = ndvi >= THRESHOLD ? Long.parseLong(time) : -1L;
            }
            reader.close();
        } catch (IOException e) {
            e.printStackTrace();
        }
        return timeSpent;
    }

    public static int calculateSecondsSpentInGreenWeek(Activity mainActivity) {
        int timeSpent = 0;
        try {
            BufferedReader reader  = new BufferedReader(new InputStreamReader(mainActivity.openFileInput("Progress_Weekly")));
            while(reader.ready()) {
                int seconds = Integer.parseInt(reader.readLine().split(": ")[1]);
                timeSpent += seconds;
            }
            reader.close();
        } catch (IOException e) {
            e.printStackTrace();
        }
        return timeSpent + calculateSecondsSpentInGreenToday(mainActivity);
    }

    public static Map<Date, Double> getActivitiesForToday(Activity mainActivity) {
        Map<Date, Double> returnValue = new HashMap<>();
        try {
            @SuppressLint("SimpleDateFormat") BufferedReader reader  = new BufferedReader(new InputStreamReader(mainActivity.openFileInput("NDVI_"+ new SimpleDateFormat("yyyy-MM-dd").format(new Date()))));
            while(reader.ready()) {
                String[] line = reader.readLine().split(": ");
                String time = line[0];
                double ndvi = Double.parseDouble(line[1]);
                returnValue.put(Date.from(Instant.ofEpochSecond(Long.parseLong(time))), ndvi);
            }
            reader.close();
        } catch (IOException e) {
            e.printStackTrace();
        }
        return returnValue;
    }
}
