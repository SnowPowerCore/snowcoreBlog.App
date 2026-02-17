using Android.Views;
using AndroidX.Core.View;
using Google.Android.Material.AppBar;
using AndroidGraphics = AndroidX.Core.Graphics;

namespace snowcoreBlog.App.Platforms.Android.Listeners;

public class CustomInsetsListener : Java.Lang.Object, IOnApplyWindowInsetsListener
{
    public WindowInsetsCompat? OnApplyWindowInsets(global::Android.Views.View? v, WindowInsetsCompat? insets)
    {
        var systemBars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
		var displayCutout = insets.GetInsets(WindowInsetsCompat.Type.DisplayCutout());

		// Handle MaterialToolbar special case early
		if (v is MaterialToolbar)
		{
			v.SetPadding(displayCutout?.Left ?? 0, 0, displayCutout?.Right ?? 0, 0);
			return WindowInsetsCompat.Consumed;
		}

		// Find AppBarLayout - check direct child first, then first two children
		var appBarLayout = v.FindViewById<AppBarLayout>(Resource.Id.navigationlayout_appbar);
		if (appBarLayout is null && v is ViewGroup group)
		{
			if (group.ChildCount > 0 && group.GetChildAt(0) is AppBarLayout firstChild)
			{
				appBarLayout = firstChild;
			}
			else if (group.ChildCount > 1 && group.GetChildAt(1) is AppBarLayout secondChild)
			{
				appBarLayout = secondChild;
			}
		}

		// Check if AppBarLayout has meaningful content
		bool appBarHasContent = appBarLayout?.MeasuredHeight > 0;
		if (!appBarHasContent && appBarLayout is not null)
		{
			for (int i = 0; i < appBarLayout.ChildCount; i++)
			{
				var child = appBarLayout.GetChildAt(i);
				if (child?.MeasuredHeight > 0)
				{
					appBarHasContent = true;
					break;
				}
			}
		}

		// Apply padding to AppBarLayout based on content and system insets
		if (appBarLayout is not null)
		{
			if (appBarHasContent)
			{
				var topInset = Math.Max(systemBars?.Top ?? 0, displayCutout?.Top ?? 0);
				appBarLayout.SetPadding(systemBars?.Left ?? 0, topInset, systemBars?.Right ?? 0, 0);
			}
			else
			{
				appBarLayout.SetPadding(0, 0, 0, 0);
			}
		}

		// Handle bottom navigation
		var hasBottomNav = v.FindViewById(Resource.Id.navigationlayout_bottomtabs)?.MeasuredHeight > 0;
		if (hasBottomNav)
		{
			var bottomInset = Math.Max(systemBars?.Bottom ?? 0, displayCutout?.Bottom ?? 0);
			v.SetPadding(0, 0, 0, bottomInset);
		}
		else
		{
			v.SetPadding(0, 0, 0, 0);
		}

		// Create new insets with consumed values
		var newSystemBars = AndroidGraphics.Insets.Of(
			systemBars?.Left ?? 0,
			appBarHasContent ? 0 : systemBars?.Top ?? 0,
			systemBars?.Right ?? 0,
			hasBottomNav ? 0 : systemBars?.Bottom ?? 0
		) ?? AndroidGraphics.Insets.None;

		var newDisplayCutout = AndroidGraphics.Insets.Of(
			displayCutout?.Left ?? 0,
			appBarHasContent ? 0 : displayCutout?.Top ?? 0,
			displayCutout?.Right ?? 0,
			hasBottomNav ? 0 : displayCutout?.Bottom ?? 0
		) ?? AndroidGraphics.Insets.None;

		return new WindowInsetsCompat.Builder(insets)
			?.SetInsets(WindowInsetsCompat.Type.SystemBars(), newSystemBars)
			?.SetInsets(WindowInsetsCompat.Type.DisplayCutout(), newDisplayCutout)
			?.Build() ?? insets;
    }
}