using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;
using AColor = Android.Graphics.Color;
using AResource = Android.Resource;
using MauiColor = Microsoft.Maui.Graphics.Color;
using ShellItem = Microsoft.Maui.Controls.ShellItem;

namespace snowcoreBlog.App.Platforms.Android.Handlers;

public class CustomShellRenderer : ShellRenderer
{
	protected override IShellItemRenderer CreateShellItemRenderer(ShellItem shellItem)
	{
		return new CustomShellItemRenderer(this);
	}

	protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
	{
		return new CustomShellBottomNavViewAppearanceTracker(this, shellItem);
	}
}

public sealed class CustomShellBottomNavViewAppearanceTracker : ShellBottomNavViewAppearanceTracker
{
	private const float CornerRadiusDp = 18f;
	private const float MarginDp = 12f;
	private const float ElevationDp = 4f;
	private const byte BackgroundAlpha = 220;

	public CustomShellBottomNavViewAppearanceTracker(IShellContext shellContext, ShellItem shellItem)
		: base(shellContext, shellItem)
	{
	}

	public override void SetAppearance(BottomNavigationView bottomView, IShellAppearanceElement appearance)
	{
		base.SetAppearance(bottomView, appearance);
		ApplySelectionColors(bottomView, appearance);
		ApplyFloatingStyle(bottomView, appearance?.EffectiveTabBarBackgroundColor);
	}

	public override void ResetAppearance(BottomNavigationView bottomView)
	{
		base.ResetAppearance(bottomView);
		ApplySelectionColors(bottomView, null);
		ApplyFloatingStyle(bottomView, null);
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

		var cornerRadiusPx = DpToPx(context, CornerRadiusDp);
		var marginPx = (int)DpToPx(context, MarginDp);
		var elevationPx = DpToPx(context, ElevationDp);

		var baseColor = backgroundColor ?? ShellRenderer.DefaultBackgroundColor;
		var platformColor = baseColor.ToPlatform();
		var tinted = AColor.Argb(BackgroundAlpha, platformColor.R, platformColor.G, platformColor.B);

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

	private static float DpToPx(Context context, float dp)
	{
		var metrics = context.Resources?.DisplayMetrics
			?? context.ApplicationContext?.Resources?.DisplayMetrics;

		if (metrics == null)
		{
			return dp;
		}

		return TypedValue.ApplyDimension(ComplexUnitType.Dip, dp, metrics);
	}
}