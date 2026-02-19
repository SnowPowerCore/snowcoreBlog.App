using Microsoft.Maui.Controls.Platform.Compatibility;
using snowcoreBlog.App.Platforms.iOS.Extensions;
using UIKit;

namespace snowcoreBlog.App.Platforms.iOS.Handlers;

public class CustomShellSectionRenderer(IShellContext context) : ShellSectionRenderer(context)
{
    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        InteractivePopGestureRecognizer.Enabled = !PageTransitions.GetDisableSwipeBackiOS(ShellSection);
    }

    public override UIViewController[] PopToRootViewController(bool animated)
    {
        Pop(animated);
        return base.PopToRootViewController(false);
    }

    public override UIViewController PopViewController(bool animated)
    {
        Pop(animated);
        return base.PopViewController(false);
    }

    private void Pop(bool animated)
    {
        if (animated)
        {
            var oldView = View.SnapshotView(false);
            var newView = View;
            var anim = PageTransitionExtensions.GetPop(ShellSection);
            View.Layer.RemoveAllAnimations();

            View.Superview.AddSubview(oldView);

            PageTransitionExtensions.Animate(oldView, anim.AnimationOut, () =>
            {
                if (oldView != null)
                {
                    oldView.Layer.RemoveAllAnimations();
                    oldView.RemoveFromSuperview();
                }
                oldView = null;
            });
            PageTransitionExtensions.Animate(newView, anim.AnimationIn);
        }
    }

    public override void PushViewController(UIViewController viewController, bool animated)
    {

        if (animated)
        {
            var oldView = View.SnapshotView(false);
            var newView = viewController.View;
            var anim = PageTransitionExtensions.GetPush(ShellSection);
            View.Layer.RemoveAllAnimations();

            View.AddSubview(newView);
            View.AddSubview(oldView);
            
            View.SendSubviewToBack(oldView);
            
            PageTransitionExtensions.Animate(oldView, anim.AnimationOut, () =>
            {
                if (oldView != null)
                {
                    oldView.Layer.RemoveAllAnimations();
                    oldView.RemoveFromSuperview();
                }
                oldView = null;
            });
            PageTransitionExtensions.Animate(newView, anim.AnimationIn);
        }
        base.PushViewController(viewController, false);
    }
}