using CustomShellMaui.Enum;
using CustomShellMaui.Platforms.Android;

namespace snowcoreBlog.App.Platforms.Android.Extensions;

public static class PageTransitionExtensions
{
    public static ConfigAndroid GetRoot(BindableObject page)
    {
        var config = new ConfigAndroid();
        var animation = PageTransitions.GetPageTransition(page);
        config.AbovePage = animation.Root.AbovePage;
        config.Duration = animation.Root.DurationAndroid;
        
            config.AnimationIn = animation.Root.NextPageAndroid <= 0
                ? GetAnimation(animation.Root.NextPage)
                : animation.Root.NextPageAndroid;

            config.AnimationOut = animation.Root.CurrentPageAndroid <= 0
                ? GetAnimation(animation.Root.CurrentPage)
                : animation.Root.CurrentPageAndroid;
                    
        return config;
    }

    public static ConfigAndroid GetPush(BindableObject page)
    {
        var config = new ConfigAndroid();
        var animation = PageTransitions.GetPageTransition(page);
        
        config.AnimationIn = animation.Push.NextPageAndroid <= 0
            ? GetAnimation(animation.Push.NextPage)
            : animation.Push.NextPageAndroid;

        config.AnimationOut = animation.Push.CurrentPageAndroid <= 0
            ? GetAnimation(animation.Push.CurrentPage)
            : animation.Push.CurrentPageAndroid;

        return config;
    }

    public static ConfigAndroid GetPop(BindableObject page)
    {
        var config = new ConfigAndroid();
        var animation = PageTransitions.GetPageTransition(page);

        config.AnimationIn = animation.Pop.NextPageAndroid <= 0
            ? GetAnimation(animation.Pop.NextPage)
            : animation.Pop.NextPageAndroid;

        config.AnimationOut = animation.Pop.CurrentPageAndroid <= 0
            ? GetAnimation(animation.Pop.CurrentPage)
            : animation.Pop.CurrentPageAndroid;

        return config;
    }

    private static int GetAnimation(TransitionType anim)
    {
        var result = anim switch
        {
            TransitionType.FadeIn => Resource.Animation.fade_in,
            TransitionType.FadeOut => Resource.Animation.fade_out,
            TransitionType.BottomIn => Resource.Animation.enter_bottom,
            TransitionType.BottomOut => Resource.Animation.exit_bottom,
            TransitionType.TopIn => Resource.Animation.enter_top,
            TransitionType.TopOut => Resource.Animation.exit_top,
            TransitionType.LeftIn => Resource.Animation.enter_left,
            TransitionType.LeftOut => Resource.Animation.exit_left,
            TransitionType.RightIn => Resource.Animation.enter_right,
            TransitionType.RightOut => Resource.Animation.exit_right,
            TransitionType.ScaleIn => Resource.Animation.scale_in,
            TransitionType.ScaleOut => Resource.Animation.scale_out,
            TransitionType.None => Resource.Animation.none,
            _ => Resource.Animation.none,
        };
        return result;
    }
}