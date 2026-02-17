using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using AndroidX.Core.View;
using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;
using snowcoreBlog.App.Platforms.Android.Extensions;
using AColor = Android.Graphics.Color;
using AResource = Android.Resource;
using AView = Android.Views.View;
using MauiColor = Microsoft.Maui.Graphics.Color;
using ShellItem = Microsoft.Maui.Controls.ShellItem;

namespace snowcoreBlog.App.Platforms.Android.Handlers;

public class CustomShellRenderer : ShellRenderer
{
	protected override IShellItemRenderer CreateShellItemRenderer(ShellItem shellItem)
	{
        var renderer = new CustomShellItemRenderer(this);
        ((IShellItemRenderer)renderer).ShellItem = shellItem;
        return renderer;
	}

    protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
    {
        return new CustomShellBottomNavViewAppearanceTracker(this, shellItem);
    }
}

public sealed class CustomShellBottomNavViewAppearanceTracker : ShellBottomNavViewAppearanceTracker
{
    private const float CornerRadiusDp = 26f;
	private const float MarginDp = 12f;
	private const float ElevationDp = 4f;

    public CustomShellBottomNavViewAppearanceTracker(IShellContext shellContext, ShellItem shellItem) : base(shellContext, shellItem) { }

	public override void SetAppearance(BottomNavigationView bottomView, IShellAppearanceElement appearance)
	{
		// base.SetAppearance(bottomView, appearance);
		// ApplySelectionColors(bottomView, appearance);
		// ApplyFloatingStyle(bottomView, appearance?.EffectiveTabBarBackgroundColor);

        // bottomView.Post(() =>
        // {
        //     ViewCompat.SetOnApplyWindowInsetsListener(bottomView, new CustomOnApplyWindowInsetsListener(ViewCompat.GetRootWindowInsets(bottomView)));
        //     ViewCompat.RequestApplyInsets(bottomView);
        // });
	}

	public override void ResetAppearance(BottomNavigationView bottomView)
	{
		base.ResetAppearance(bottomView);
		ApplySelectionColors(bottomView, null);
		ApplyFloatingStyle(bottomView, null);

        bottomView.Post(() =>
        {
            ViewCompat.SetOnApplyWindowInsetsListener(bottomView, new CustomOnApplyWindowInsetsListener(ViewCompat.GetRootWindowInsets(bottomView)));
            ViewCompat.RequestApplyInsets(bottomView);
        });
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
		var marginPx = (int)ViewDimensionExtensions.DpToPx(context, MarginDp);
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

		if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
		{
			bottomView.ClipToOutline = false;
			bottomView.OutlineProvider = ViewOutlineProvider.Background;
		}

		if (bottomView.LayoutParameters is ViewGroup.MarginLayoutParams marginLayoutParams)
		{
			marginLayoutParams.SetMargins(marginPx + marginPx, marginPx, marginPx + marginPx, marginPx);
			bottomView.LayoutParameters = marginLayoutParams;
		}
		else if (bottomView.LayoutParameters != null)
		{
			var newLayoutParams = new ViewGroup.MarginLayoutParams(bottomView.LayoutParameters);
			newLayoutParams.SetMargins(marginPx + marginPx, marginPx, marginPx + marginPx, marginPx);
			bottomView.LayoutParameters = newLayoutParams;
		}

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

	private class CustomOnApplyWindowInsetsListener : Java.Lang.Object, IOnApplyWindowInsetsListener
    {
    	private readonly int _systemBarsInsetsType = WindowInsets.Type.SystemBars();
    	private readonly int _displayCutoutInsetsType = WindowInsets.Type.DisplayCutout();

		private int? _existingLeftMargin;
		private int? _existingRightMargin;
		private int? _existingTopMargin;
		private int? _existingBottomMargin;

		private readonly WindowInsetsCompat _rootWindowInsets;

		public CustomOnApplyWindowInsetsListener(WindowInsetsCompat rootWindowInsets)
		{
			_rootWindowInsets = rootWindowInsets;
		}

        public WindowInsetsCompat OnApplyWindowInsets(AView v, WindowInsetsCompat insets)
        {
			var systemBars = _rootWindowInsets.GetInsets(_systemBarsInsetsType);
			var displayCutout = _rootWindowInsets.GetInsets(_displayCutoutInsetsType);

			var left = systemBars.Left + displayCutout.Left;
			var top = systemBars.Top + displayCutout.Top;
			var right = systemBars.Right + displayCutout.Right;
			var bottom = systemBars.Bottom + displayCutout.Bottom;

			// Add insets to existing margins instead of replacing them, so layout shifts correctly.
			if (v.LayoutParameters is ViewGroup.MarginLayoutParams existingMargins)
			{
				_existingLeftMargin ??= existingMargins.LeftMargin;
				_existingTopMargin ??= existingMargins.TopMargin;
				_existingRightMargin ??= existingMargins.RightMargin;
				_existingBottomMargin ??= existingMargins.BottomMargin;
				existingMargins.SetMargins(_existingLeftMargin.Value + left, _existingTopMargin.Value + top, _existingRightMargin.Value + right, _existingBottomMargin.Value + bottom);
				v.LayoutParameters = existingMargins;
			}
			else if (v.LayoutParameters != null)
			{
				var newLayoutParams = new ViewGroup.MarginLayoutParams(v.LayoutParameters);
				// No existing margins available in this case; apply the insets as margins.
				newLayoutParams.SetMargins(left, top, right, bottom);
				v.LayoutParameters = newLayoutParams;
			}
			else
			{
				var newLayoutParams = new ViewGroup.MarginLayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
				newLayoutParams.SetMargins(left, top, right, bottom);
				v.LayoutParameters = newLayoutParams;
			}

			v.RequestLayout();
			v.Invalidate();

			return insets;
        }
    }
}