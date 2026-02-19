using CustomShellMaui.Enum;
using CustomShellMaui.Models;

namespace snowcoreBlog.App;

public partial class PageTransitions : BindableObject
{
    public static readonly BindableProperty DisableSwipeBackiOSProperty = BindableProperty.CreateAttached("DisableSwipeBackiOS", typeof(bool), typeof(Insets), false);
    public static readonly BindableProperty PageTransitionProperty = BindableProperty.CreateAttached("PageTransition", typeof(Transitions), typeof(Insets), new Transitions
    {
        Root = new TransitionRoot
        {
            CurrentPage = TransitionType.FadeOut
        },
        Push = new Transition
        {
#if ANDROID
            DurationAndroid = 50,
#endif
            CurrentPage = TransitionType.LeftOut,
            NextPage = TransitionType.RightIn
        },
        Pop = new Transition
        {
#if ANDROID
            DurationAndroid = 50,
#endif
            CurrentPage = TransitionType.RightOut,
            NextPage = TransitionType.LeftIn
        },
    });

    public static bool GetDisableSwipeBackiOS(BindableObject target)
    {
        return (bool)target.GetValue(DisableSwipeBackiOSProperty);
    }

    public static void SetDisableSwipeBackiOS(BindableObject target, bool value)
    {
        target.SetValue(DisableSwipeBackiOSProperty, value);
    }

    public static Transitions GetPageTransition(BindableObject target)
    {
        return (Transitions)target.GetValue(PageTransitionProperty);
    }

    public static void SetPageTransition(BindableObject target, Transitions value)
    {
        target.SetValue(PageTransitionProperty, value);
    }
}