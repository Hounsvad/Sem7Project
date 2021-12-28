package dk.sdu.mmmi.sempro7E21.grpG.outinthegreen.ui.settings;

import android.os.Bundle;
import android.text.InputType;
import android.widget.EditText;

import androidx.annotation.NonNull;
import androidx.preference.EditTextPreference;
import androidx.preference.PreferenceFragmentCompat;

import java.util.Objects;

import dk.sdu.mmmi.sempro7E21.grpG.outinthegreen.R;

public class SettingsFragment extends PreferenceFragmentCompat  {

    @Override
    public void onCreatePreferences(Bundle savedInstanceState, String rootKey) {


        EditTextPreference dailyHoursPref = findPreference("daily");

        if (dailyHoursPref != null) {
            dailyHoursPref.setOnBindEditTextListener(
                    editText -> editText.setInputType(InputType.TYPE_CLASS_NUMBER));
        }

        EditTextPreference weeklyHoursPref = findPreference("weekly_hours_pref");

        if (weeklyHoursPref != null) {
            weeklyHoursPref.setOnBindEditTextListener(
                    editText -> editText.setInputType(InputType.TYPE_CLASS_NUMBER));
        }

        setPreferencesFromResource(R.xml.preferences, rootKey);
    }

}
