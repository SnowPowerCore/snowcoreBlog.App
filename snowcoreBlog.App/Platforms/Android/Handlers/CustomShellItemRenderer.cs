using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls.Platform.Compatibility;
using AView = Android.Views.View;
using AColor = Android.Graphics.Color;

namespace snowcoreBlog.App.Platforms.Android.Handlers;

public class CustomShellItemRenderer : ShellItemRenderer
{
    private const float InitialMarginDp = 12f;

    public CustomShellItemRenderer(IShellContext shellContext) : base(shellContext) { }

    public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        var root = base.OnCreateView(inflater, container, savedInstanceState);
        if (root is ViewGroup rootGroup)
        {
            TryApplyOverlayLayout(rootGroup);
        }

        return root;
    }

    private static void TryApplyOverlayLayout(ViewGroup root)
    {
        BottomNavigationView bottomView = null;
        ViewGroup navigationArea = null;

        for (var i = 0; i < root.ChildCount; i++)
        {
            var child = root.GetChildAt(i);
            if (child is BottomNavigationView bottom)
            {
                bottomView = bottom;
            }
            else if (child is ViewGroup group)
            {
                navigationArea = group;
            }
        }

        if (bottomView == null || navigationArea == null)
        {
            return;
        }

        root.RemoveAllViews();

        var overlay = new FrameLayout(root.Context)
        {
            LayoutParameters = new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent)
        };

        navigationArea.SetBackgroundColor(AColor.Transparent);
        overlay.AddView(
            navigationArea,
            new FrameLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent));
        overlay.AddView(
            bottomView,
            new FrameLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent,
                GravityFlags.Bottom));

        root.AddView(overlay);

        ApplyInitialBottomBarMargins(bottomView);
    }

    private static void ApplyInitialBottomBarMargins(BottomNavigationView bottomView)
    {
        var context = bottomView.Context;
        var metrics = context?.Resources?.DisplayMetrics
            ?? context?.ApplicationContext?.Resources?.DisplayMetrics;

        if (metrics == null)
        {
            return;
        }

        var marginPx = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, InitialMarginDp, metrics);
        if (bottomView.LayoutParameters is ViewGroup.MarginLayoutParams marginLayoutParams)
        {
            marginLayoutParams.SetMargins(marginPx, marginPx, marginPx, marginPx);
            bottomView.LayoutParameters = marginLayoutParams;
            bottomView.RequestLayout();
        }
    }
}
