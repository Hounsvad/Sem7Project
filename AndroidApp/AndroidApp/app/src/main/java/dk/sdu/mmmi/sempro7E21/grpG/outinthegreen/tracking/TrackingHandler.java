package dk.sdu.mmmi.sempro7E21.grpG.outinthegreen.tracking;

import android.Manifest;
import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.Context;
import android.content.pm.PackageManager;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.os.Build;
import android.os.Bundle;

import androidx.annotation.RequiresApi;

@RequiresApi(api = Build.VERSION_CODES.Q)
public class TrackingHandler {
    private final Activity mainActivity;
    private final LocationManager locationManager;
    private final ApiHelper apiHelper;

    final String[] permissions = {
            Manifest.permission.ACCESS_FINE_LOCATION, Manifest.permission.ACCESS_COARSE_LOCATION, Manifest.permission.ACCESS_BACKGROUND_LOCATION
    };

    final int requestCode = 0x68339;

    public TrackingHandler(Activity mainActivity) {
        this.mainActivity = mainActivity;
        this.apiHelper = ApiHelper.getInstance();
        this.locationManager = (LocationManager) mainActivity.getSystemService(Context.LOCATION_SERVICE);
    }

    @SuppressLint("MissingPermission")
    public void handle() {
        if (mainActivity.checkSelfPermission(Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED
                && mainActivity.checkSelfPermission(Manifest.permission.ACCESS_COARSE_LOCATION) != PackageManager.PERMISSION_GRANTED
                && mainActivity.checkSelfPermission(Manifest.permission.ACCESS_BACKGROUND_LOCATION) != PackageManager.PERMISSION_GRANTED) {
            mainActivity.requestPermissions(permissions, requestCode);
        } else {
            locationManager.requestLocationUpdates(LocationManager.GPS_PROVIDER, 1000, 2, mLocationListener);
        }
    }

    private final LocationListener mLocationListener = new LocationListener() {
        @Override
        public void onLocationChanged(final Location location) {
            LocalStorageHelper.storeNDVI(apiHelper.getNDVI(location.getLatitude(), location.getLongitude()), mainActivity);
        }

        @Override
        public void onStatusChanged(String provider, int status, Bundle extras) {
            //NOOP
        }

        @Override
        public void onProviderEnabled(String provider) {
            //NOOP
        }

        @Override
        public void onProviderDisabled(String provider) {
            //NOOP
        }
    };
}
