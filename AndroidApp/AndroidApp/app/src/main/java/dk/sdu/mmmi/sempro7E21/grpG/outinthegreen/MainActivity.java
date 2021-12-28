package dk.sdu.mmmi.sempro7E21.grpG.outinthegreen;

import android.Manifest;
import android.annotation.SuppressLint;
import android.app.AlarmManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.os.Build;
import android.os.Bundle;
import android.view.Menu;
import android.view.MenuItem;

import com.google.android.material.navigation.NavigationView;

import androidx.annotation.NonNull;
import androidx.annotation.RequiresApi;
import androidx.navigation.NavController;
import androidx.navigation.Navigation;
import androidx.navigation.ui.AppBarConfiguration;
import androidx.navigation.ui.NavigationUI;
import androidx.drawerlayout.widget.DrawerLayout;
import androidx.appcompat.app.AppCompatActivity;

import java.time.LocalDate;
import java.time.LocalDateTime;
import java.time.ZoneOffset;
import java.util.Date;

import dk.sdu.mmmi.sempro7E21.grpG.outinthegreen.databinding.ActivityMainBinding;
import dk.sdu.mmmi.sempro7E21.grpG.outinthegreen.tracking.AlarmReceiver;
import dk.sdu.mmmi.sempro7E21.grpG.outinthegreen.tracking.TrackingHandler;
import dk.sdu.mmmi.sempro7E21.grpG.outinthegreen.ui.settings.SettingsActivity;
import dk.sdu.mmmi.sempro7E21.grpG.outinthegreen.ui.settings.SettingsFragment;

public class MainActivity extends AppCompatActivity {

    private AppBarConfiguration mAppBarConfiguration;
    private ActivityMainBinding binding;
    private TrackingHandler trackingHandler;
    private AlarmManager alarmManager;

    @SuppressLint("MissingPermission")
    @RequiresApi(api = Build.VERSION_CODES.S)
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        binding = ActivityMainBinding.inflate(getLayoutInflater());
        setContentView(binding.getRoot());

        setSupportActionBar(binding.appBarMain.toolbar);

        DrawerLayout drawer = binding.drawerLayout;
        NavigationView navigationView = binding.navView;
        // Passing each menu ID as a set of Ids because each
        // menu should be considered as top level destinations.
        mAppBarConfiguration = new AppBarConfiguration.Builder(
                R.id.nav_home, R.id.nav_stats, R.id.nav_map)
                .setOpenableLayout(drawer)
                .build();
        NavController navController = Navigation.findNavController(this, R.id.nav_host_fragment_content_main);
        NavigationUI.setupActionBarWithNavController(this, navController, mAppBarConfiguration);
        NavigationUI.setupWithNavController(navigationView, navController);

        this.trackingHandler = new TrackingHandler(this);
        trackingHandler.handle();

        this.alarmManager = ( AlarmManager) this.getSystemService(ALARM_SERVICE);
        if (! this.alarmManager.canScheduleExactAlarms()){
            this.requestPermissions(new String[]{Manifest.permission.SCHEDULE_EXACT_ALARM}, 1);
        }

        LocalDateTime localDateTime = LocalDateTime.now().withHour(0).withMinute(0).withSecond(0).withNano(0);
        long nextMidnightMillis = localDateTime.isBefore(LocalDateTime.now()) ? localDateTime.plusDays(1).toEpochSecond(ZoneOffset.UTC) : localDateTime.toEpochSecond(ZoneOffset.UTC);

        Intent intent = new Intent(this, AlarmReceiver.class);
        @SuppressLint("UnspecifiedImmutableFlag") PendingIntent pendingIntent = PendingIntent.getBroadcast(getApplicationContext(), 0, intent, PendingIntent.FLAG_UPDATE_CURRENT);
        this.alarmManager.setExactAndAllowWhileIdle(AlarmManager.RTC,
                nextMidnightMillis,
                pendingIntent
        );
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.main, menu);
        return true;
    }

    @Override
    public boolean onSupportNavigateUp() {
        NavController navController = Navigation.findNavController(this, R.id.nav_host_fragment_content_main);
        return NavigationUI.navigateUp(navController, mAppBarConfiguration)
                || super.onSupportNavigateUp();
    }

    @Override
    public boolean onOptionsItemSelected(@NonNull MenuItem item) {
        if (item.getItemId() == R.id.action_settings) {
            Intent intent = new Intent(this, SettingsActivity.class);
            startActivity(intent);
            return true;
        }
        return super.onOptionsItemSelected(item);
    }
}