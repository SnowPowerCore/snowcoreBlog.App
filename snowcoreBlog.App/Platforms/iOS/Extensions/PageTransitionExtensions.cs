using CoreAnimation;
using CoreGraphics;
using CustomShellMaui.Enum;
using CustomShellMaui.Platforms.iOS;
using Microsoft.Maui.Platform;
using UIKit;

namespace snowcoreBlog.App.Platforms.iOS.Extensions;

public class PageTransitionExtensions
{
    public static ConfigiOSTransitions GetRoot(BindableObject page)
    {
        var config = new ConfigiOSTransitions();
        var animation = PageTransitions.GetPageTransition(page);
        config.AbovePage = animation.Root.AbovePage;
        
        config.AnimationIn = animation.Root.NextPageiOS ?? GetAnimation(animation.Root.NextPage) ?? new ConfigiOS();
        config.AnimationOut = animation.Root.CurrentPageiOS ?? GetAnimation(animation.Root.CurrentPage) ?? new ConfigiOS();

        return config;
    }

    public static ConfigiOSTransitions GetPush(BindableObject page)
    {
        var config = new ConfigiOSTransitions();
        var animation = PageTransitions.GetPageTransition(page);
        config.AnimationIn = animation.Push.NextPageiOS ?? GetAnimation(animation.Push.NextPage) ?? new ConfigiOS();
        config.AnimationOut = animation.Push.CurrentPageiOS ?? GetAnimation(animation.Push.CurrentPage) ?? new ConfigiOS();
        return config;
    }

    public static ConfigiOSTransitions GetPop(BindableObject page)
    {
        var config = new ConfigiOSTransitions();
        var animation = PageTransitions.GetPageTransition(page);
        config.AnimationIn = animation.Pop.NextPageiOS ?? GetAnimation(animation.Pop.NextPage) ?? new ConfigiOS();
        config.AnimationOut = animation.Pop.CurrentPageiOS ?? GetAnimation(animation.Pop.CurrentPage) ?? new ConfigiOS();
        return config;
    }

    private static ConfigiOS GetAnimation(TransitionType anim)
    {
        ConfigiOS result = anim switch
        {
            TransitionType.FadeIn => new ConfigiOS
            {
                OpacityStart = 0,
                OpacityEnd = 1
            },
            TransitionType.FadeOut => new ConfigiOS
            {
                OpacityStart = 1,
                OpacityEnd = 0
            },
            TransitionType.BottomIn => new ConfigiOS
            {
                YStart = 1,
                YEnd = 0
            },
            TransitionType.BottomOut => new ConfigiOS
            {
                YStart = 0,
                YEnd = 1
            },
            TransitionType.TopIn => new ConfigiOS
            {
                YStart = -1,
                YEnd = 0
            },
            TransitionType.TopOut => new ConfigiOS
            {
                YStart = 0,
                YEnd = -1
            },
            TransitionType.LeftIn => new ConfigiOS
            {
                XStart = -1,
                XEnd = 0
            },
            TransitionType.LeftOut => new ConfigiOS
            {
                XStart = 0,
                XEnd = -1
            },
            TransitionType.RightIn => new ConfigiOS
            {
                XStart = 1,
                XEnd = 0
            },
            TransitionType.RightOut => new ConfigiOS
            {
                XStart = 0,
                XEnd = 1
            },
            TransitionType.ScaleIn => new ConfigiOS
            {
                ScaleStart = 1.5,
                ScaleEnd = 1,
                OpacityStart = 0,
                OpacityEnd = 1
            },
            TransitionType.ScaleOut => new ConfigiOS
            {
                ScaleStart = 1,
                ScaleEnd = 1.5,
                OpacityStart = 1,
                OpacityEnd = 0
            },
            TransitionType.None => new ConfigiOS
            {
                OpacityStart = 1,
                OpacityEnd = 1.1
            },
            _ => new ConfigiOS
            {
                OpacityStart = 1,
                OpacityEnd = 1.1
            },
        };

        return result;
    }

    public static void Animate(UIView view, ConfigiOS config, Action callBack = null)
    {
        view.Layer.Opacity = (float)config.OpacityStart;
        var trans = CGAffineTransform.MakeTranslation(PosX(config.XStart, view), PosY(config.YStart, view));
        var scale = CGAffineTransform.MakeScale((float)config.ScaleStart, (float)config.ScaleStart);
        var rotation = CGAffineTransform.MakeRotation(PosRotation(config.RotationStart));
        trans.Multiply(scale);
        trans.Multiply(rotation);
        view.Transform = trans;

        UIView.Animate(config.Duration, 0, UIViewAnimationOptions.CurveEaseInOut,
            () =>
            {
            var scale = CGAffineTransform.MakeScale((float)config.ScaleEnd, (float)config.ScaleEnd);
                var trans = CGAffineTransform.MakeTranslation(PosX(config.XEnd, view), PosY(config.YEnd, view));
                var rotation = CGAffineTransform.MakeRotation(PosRotation(config.RotationEnd));
                trans.Multiply(scale);
                trans.Multiply(rotation);
                view.Transform = trans;
                view.Layer.Opacity = (float)config.OpacityEnd;
            }, callBack
        );
    }

    //TODO WILL BE ABANDONED AND IMPROVED OVER TIME
    public static void FixToStart(UIView view, double duration = 0.5)
    {
        var transition = CATransition.CreateAnimation();

        transition.Duration = duration;
        transition.Type = CAAnimation.TransitionMoveIn;
        transition.Subtype = CAAnimation.TransitionFromRight;
        transition.StartProgress = (float)0.5;
        transition.EndProgress = (float)0.5;
        view.Layer.AddAnimation(transition, null);
        view.BackgroundColor = Colors.Transparent.ToPlatform();
        view.ClipsToBounds = false;

        var fixAnimation = CABasicAnimation.FromKeyPath("transform.translation.x");
        fixAnimation.From = Foundation.NSNumber.FromDouble(-view.Bounds.Width / 2);
        fixAnimation.To = Foundation.NSNumber.FromDouble(-view.Bounds.Width / 2);
        fixAnimation.Duration = duration;
        fixAnimation.AnimationStopped += (sender, e) =>
        {
            view.Layer.Transform = CATransform3D.MakeTranslation(0, 0, 0);
        };
        view.Layer.AddAnimation(fixAnimation, null);
    }

    private static float PosRotation(double value)
    {
        float result = (float)(2 * Math.PI * value);
        return result;
    }

    private static float PosX(double value, UIView view)
    {
        float result = (float)(view.Bounds.Width * value);
        return result;
    }

    private static float PosY(double value, UIView view)
    {
        float result = (float)(view.Bounds.Height * value);
        return result;
    }
}