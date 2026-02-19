using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using CustomShellMaui.Enum;
using CustomShellMaui.Platforms.Android;
using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;
using snowcoreBlog.App.Platforms.Android.Extensions;
using AColor = Android.Graphics.Color;
using AResource = Android.Resource;
using MauiColor = Microsoft.Maui.Graphics.Color;
using ShellItem = Microsoft.Maui.Controls.ShellItem;

namespace snowcoreBlog.App.Platforms.Android.Handlers;

public class CustomShellRenderer2 : ShellRenderer
{
	private IShellItemRenderer _currentView;

    protected override IShellItemRenderer CreateShellItemRenderer(ShellItem shellItem)
    {
        var renderer = new CustomShellItemRenderer2(this);
        ((IShellItemRenderer)renderer).ShellItem = shellItem;
        return renderer;
    }

    protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem) =>
        new CustomShellBottomNavViewAppearanceTracker2(this, shellItem);

	protected override void SwitchFragment(FragmentManager manager, global::Android.Views.View targetView, ShellItem newItem, bool animate = true)
	{
		var animation = PageTransitionExtensions.GetRoot(newItem);

		if (!animate || animation is default(ConfigAndroid))
		{
			base.SwitchFragment(manager, targetView, newItem, false);
			return;
		}

		var previousView = _currentView;
		_currentView = CreateShellItemRenderer(newItem);
		_currentView.ShellItem = newItem;
		var fragment = _currentView.Fragment;

		FragmentTransaction transaction = manager.BeginTransaction();

		if (animate)
			transaction.SetCustomAnimations(animation.AnimationIn, animation.AnimationOut);

		if (animation.AbovePage == TransitionPageType.NextPage && animate)
		{
			transaction.Add(targetView.Id, fragment);
			Task.Run(async () =>
			{
				await Task.Delay(animation.Duration);
				FragmentTransaction transactionTemp = manager.BeginTransaction();
				transactionTemp.Replace(fragment.Id, fragment);
				transactionTemp.CommitAllowingStateLoss();
			});
		}
		else
		{
			transaction.Replace(targetView.Id, fragment);
		}

		if (previousView is default(IShellItemRenderer))
		{
			transaction.SetReorderingAllowed(true);
		}

		transaction.CommitAllowingStateLoss();


		void OnDestroyed(object sender, EventArgs args)
		{
			if (previousView is not default(IShellItemRenderer))
			{
				previousView.Destroyed -= OnDestroyed;
				previousView.Dispose();
				previousView = default;
			}
		}

		if (previousView is not default(IShellItemRenderer))
			previousView.Destroyed += OnDestroyed;
	}
}

internal class CustomShellBottomNavViewAppearanceTracker2(IShellContext shellContext, ShellItem shellItem) : ShellBottomNavViewAppearanceTracker(shellContext, shellItem)
{
    private const float CornerRadiusDp = 26f;
	private const float ElevationDp = 4f;

    public override void ResetAppearance(BottomNavigationView bottomView)
    {
		base.ResetAppearance(bottomView);
		ApplySelectionColors(bottomView, default);
        ApplyFloatingStyle(bottomView, default);
    }

    private static void ApplyFloatingStyle(BottomNavigationView bottomView, MauiColor? backgroundColor)
	{
		var context = bottomView.Context;
		if (context == null)
		{
			return;
		}

		if (bottomView.LayoutParameters == null)
		{
			bottomView.Post(() => ApplyFloatingStyle(bottomView, backgroundColor));
			return;
		}

		var cornerRadiusPx = ViewDimensionExtensions.DpToPx(context, CornerRadiusDp);
		var elevationPx = ViewDimensionExtensions.DpToPx(context, ElevationDp);

		var baseColor = backgroundColor ?? ShellRenderer.DefaultBackgroundColor;
		var platformColor = baseColor.ToPlatform();
		var tinted = AColor.Argb(240, platformColor.R, platformColor.G, platformColor.B);

		var background = new GradientDrawable();
		background.SetColor(tinted);
		background.SetCornerRadius(cornerRadiusPx);
		bottomView.Background = background;
		bottomView.Elevation = elevationPx;
		bottomView.TranslationZ = elevationPx;
		bottomView.ItemRippleColor = ColorStateList.ValueOf(AColor.Transparent);
		bottomView.ItemBackground = null;
		bottomView.SetPadding(0, 0, 0, 0);
		bottomView.SetClipToPadding(false);
		bottomView.SetClipChildren(false);
		DisableItemRipples(bottomView);

        bottomView.ClipToOutline = false;
        bottomView.OutlineProvider = ViewOutlineProvider.Background;

		bottomView.Invalidate();
	}

	private static void ApplySelectionColors(BottomNavigationView bottomView, IShellAppearanceElement? appearance)
	{
		var selected = appearance?.EffectiveTabBarForegroundColor ?? ShellRenderer.DefaultForegroundColor;
		var unselected = appearance?.EffectiveTabBarUnselectedColor ?? ShellRenderer.DefaultUnselectedColor;
		var disabled = appearance?.EffectiveTabBarDisabledColor ?? Colors.Gray;

		var selectedArgb = selected.ToPlatform().ToArgb();
		var unselectedArgb = unselected.ToPlatform().ToArgb();
		var disabledArgb = disabled.ToPlatform().ToArgb();

		var states = new int[][]
		{
			[-AResource.Attribute.StateEnabled],
			[AResource.Attribute.StateChecked],
            []
		};
		var colors = new[] { disabledArgb, selectedArgb, unselectedArgb };
		var colorStateList = new ColorStateList(states, colors);
		bottomView.ItemIconTintList = colorStateList;
		bottomView.ItemTextColor = colorStateList;
	}

	private static void DisableItemRipples(BottomNavigationView bottomView)
	{
		if (bottomView.GetChildAt(0) is not ViewGroup menuView)
		{
			return;
		}

		for (var i = 0; i < menuView.ChildCount; i++)
		{
			var itemView = menuView.GetChildAt(i);
			if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
			{
				itemView.Foreground = null;
			}
			itemView.Background = null;
		}
	}
}