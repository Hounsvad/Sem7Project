package dk.sdu.mmmi.sempro7E21.grpG.outinthegreen.ui.map;

import android.annotation.SuppressLint;
import android.content.Context;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;

import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;

import com.google.android.gms.maps.CameraUpdate;
import com.google.android.gms.maps.CameraUpdateFactory;
import com.google.android.gms.maps.GoogleMap;
import com.google.android.gms.maps.GoogleMap.OnCameraIdleListener;
import com.google.android.gms.maps.OnMapReadyCallback;
import com.google.android.gms.maps.SupportMapFragment;
import com.google.android.gms.maps.model.Dash;
import com.google.android.gms.maps.model.Gap;
import com.google.android.gms.maps.model.LatLng;
import com.google.android.gms.maps.model.PatternItem;
import com.google.android.gms.maps.model.Polygon;
import com.google.android.gms.maps.model.PolygonOptions;

import java.util.Arrays;
import java.util.List;

import dk.sdu.mmmi.sempro7E21.grpG.outinthegreen.R;
import dk.sdu.mmmi.sempro7E21.grpG.outinthegreen.databinding.FragmentMapBinding;
import dk.sdu.mmmi.sempro7E21.grpG.outinthegreen.tracking.ApiHelper;


public class MapFragment extends Fragment implements
        OnMapReadyCallback,
        LocationListener,
        OnCameraIdleListener {

    private FragmentMapBinding binding;
    private LocationManager locationManager;
    private GoogleMap googleMap;
    private static final long MIN_TIME = 400;
    private static final float MIN_DISTANCE = 1000;

    @SuppressLint("MissingPermission")
    public View onCreateView(@NonNull LayoutInflater inflater,
                             ViewGroup container, Bundle savedInstanceState) {

        binding = FragmentMapBinding.inflate(inflater, container, false);
        View root = binding.getRoot();
        SupportMapFragment mapFragment = (SupportMapFragment) getChildFragmentManager().findFragmentById(R.id.map);
        mapFragment.getMapAsync(this);
        locationManager = (LocationManager) getActivity().getSystemService(Context.LOCATION_SERVICE);
        locationManager.requestLocationUpdates(LocationManager.NETWORK_PROVIDER, MIN_TIME, MIN_DISTANCE, this);
        return root;
    }

    @Override
    public void onLocationChanged(Location location) {
        LatLng latLng = new LatLng(location.getLatitude(), location.getLongitude());
        CameraUpdate cameraUpdate = CameraUpdateFactory.newLatLngZoom(latLng, 10);
        googleMap.animateCamera(cameraUpdate);
        locationManager.removeUpdates(this);
        updateOverlay();
    }

    @Override
    public void onMapReady(@NonNull GoogleMap googleMap) {
        this.googleMap = googleMap;
        //googleMap.moveCamera(CameraUpdateFactory.newLatLngZoom(new LatLng(-23.684, 133.903), 4));
    }

    @Override
    public void onCameraIdle() {
        updateOverlay();
    }

    private void updateOverlay(){
        this.googleMap.clear();
        for (double[][] coordinates : ApiHelper.getInstance().getOverlayCoordinates(this.googleMap.getProjection().getVisibleRegion())){
            Polygon polygon = googleMap.addPolygon(new PolygonOptions()
                    .clickable(true)
                    .add(Arrays.stream(coordinates).map(c -> new LatLng(c[0], c[1])).toArray(LatLng[]::new)));
            stylePolygon(polygon);
        }
    }

    private static final int PATTERN_GAP_LENGTH_PX = 10;
    private static final PatternItem GAP = new Gap(PATTERN_GAP_LENGTH_PX);

    private static final int COLOR_DARK_GREEN_ARGB = 0x85388E3C;
    private static final int COLOR_LIGHT_GREEN_ARGB = 0x6581C784;

    private static final int POLYGON_STROKE_WIDTH_PX = 4;
    private static final int PATTERN_DASH_LENGTH_PX = 20;
    private static final PatternItem DASH = new Dash(PATTERN_DASH_LENGTH_PX);

    // Create a stroke pattern of a gap followed by a dash.
    private static final List<PatternItem> PATTERN_POLYGON = Arrays.asList(GAP, DASH);

    private void stylePolygon(Polygon polygon) {
        polygon.setStrokePattern(PATTERN_POLYGON);
        polygon.setStrokeWidth(POLYGON_STROKE_WIDTH_PX);
        polygon.setStrokeColor(COLOR_DARK_GREEN_ARGB);
        polygon.setFillColor(COLOR_LIGHT_GREEN_ARGB);
    }

    @Override
    public void onStatusChanged(String provider, int status, Bundle extras) { }

    @Override
    public void onProviderEnabled(String provider) { }

    @Override
    public void onProviderDisabled(String provider) { }


}