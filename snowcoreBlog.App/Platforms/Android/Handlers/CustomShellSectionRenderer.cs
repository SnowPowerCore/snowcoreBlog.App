using Android.OS;
using Android.Views;
using Microsoft.Maui.Controls.Platform.Compatibility;
using AViews = Android.Views;

namespace snowcoreBlog.App.Platforms.Android.Handlers;

public class CustomShellSectionRenderer : ShellSectionRenderer
{
    public CustomShellSectionRenderer(IShellContext shellContext) : base(shellContext) {}

    public override AViews.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        var result =  base.OnCreateView(inflater, container, savedInstanceState);
        SetViewPager2UserInputEnabled(true);
        return result;
    }

    protected override void SetViewPager2UserInputEnabled(bool value)
    {
        base.SetViewPager2UserInputEnabled(true);
    }
}