using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace snowcoreBlog.App;

[Activity(Label = "@string/app_name",
          Theme = "@style/CustomTheme",
          MainLauncher = true,
          LaunchMode = LaunchMode.SingleTask,
          ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation
            | ConfigChanges.UiMode | ConfigChanges.ScreenLayout
            | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        var s = SystemUiFlags.LayoutFullscreen | SystemUiFlags.LayoutStable;
        FindViewById(Android.Resource.Id.Content).SystemUiVisibility = (StatusBarVisibility)s;
    }
}