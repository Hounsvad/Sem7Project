package dk.sdu.mmmi.sempro7E21.grpG.outinthegreen.ui.stats;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.Observer;
import androidx.lifecycle.ViewModelProvider;

import com.jjoe64.graphview.GraphView;
import com.jjoe64.graphview.series.DataPoint;
import com.jjoe64.graphview.series.LineGraphSeries;

import java.time.LocalDateTime;
import java.time.ZoneId;
import java.time.ZoneOffset;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.stream.Collectors;

import dk.sdu.mmmi.sempro7E21.grpG.outinthegreen.databinding.FragmentStatsBinding;
import dk.sdu.mmmi.sempro7E21.grpG.outinthegreen.tracking.LocalStorageHelper;

public class StatsFragment extends Fragment {

    private FragmentStatsBinding binding;

    public View onCreateView(@NonNull LayoutInflater inflater,
                             ViewGroup container, Bundle savedInstanceState) {

        binding = FragmentStatsBinding.inflate(inflater, container, false);
        View root = binding.getRoot();

        final GraphView graph = binding.graph;
        /*DataPoint[] points = LocalStorageHelper.getActivitiesForToday(this.requireActivity())
                .entrySet()
                .stream()
                .map(e -> new DataPoint(LocalDateTime.ofInstant(e.getKey().toInstant(), ZoneOffset.UTC).getHour(), e.getValue()))
                .collect(Collectors.groupingBy(DataPoint::getX, Collectors.averagingDouble(DataPoint::getY)))
                .entrySet()
                .stream()
                .map(e -> new DataPoint(e.getKey(), e.getValue()))
                .toArray(DataPoint[]::new);*/

        DataPoint[] points = {
                new DataPoint(9, 0.5),
                new DataPoint(10, 0.1),
                new DataPoint(11, 0.2),
                new DataPoint(12, 0),
                new DataPoint(13, 0.6),
                new DataPoint(14, 0),
                new DataPoint(15, 0),
                new DataPoint(16, 0.4),
                new DataPoint(17, 0.075)
        };

        LineGraphSeries<DataPoint> series = new LineGraphSeries<>(points);

        graph.addSeries(series);
        return root;
    }

    @Override
    public void onDestroyView() {
        super.onDestroyView();
        binding = null;
    }
}