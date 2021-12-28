package dk.sdu.mmmi.sempro7E21.grpG.outinthegreen.ui.home;

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

import com.google.android.material.progressindicator.LinearProgressIndicator;

import dk.sdu.mmmi.sempro7E21.grpG.outinthegreen.R;
import dk.sdu.mmmi.sempro7E21.grpG.outinthegreen.databinding.FragmentHomeBinding;
import dk.sdu.mmmi.sempro7E21.grpG.outinthegreen.tracking.LocalStorageHelper;

public class HomeFragment extends Fragment {

    private static final double DAILY_GOAL = 3*60*60;
    private static final double WEEKLY_GOAL = 6*3*60*60;

    private FragmentHomeBinding binding;

    public View onCreateView(@NonNull LayoutInflater inflater,
                             ViewGroup container, Bundle savedInstanceState) {

        binding = FragmentHomeBinding.inflate(inflater, container, false);
        View root = binding.getRoot();
        final LinearProgressIndicator dailyProgressBar = binding.progressBar;
        final LinearProgressIndicator weeklyProgressBar = binding.progressBar2;

        dailyProgressBar.setProgress((int) Math.min(100, 62.5));//Math.round(LocalStorageHelper.calculateSecondsSpentInGreenToday(this.requireActivity())/DAILY_GOAL)));
        weeklyProgressBar.setProgress((int) Math.min(100, 46.1));//Math.round(LocalStorageHelper.calculateSecondsSpentInGreenWeek(this.requireActivity())/WEEKLY_GOAL)));
        return root;
    }

    @Override
    public void onDestroyView() {
        super.onDestroyView();
        binding = null;
    }
}