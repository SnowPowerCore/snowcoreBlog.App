using CustomShellMaui.Enum;
using Microsoft.Maui.Controls.Platform.Compatibility;
using snowcoreBlog.App.Platforms.iOS.Extensions;

namespace snowcoreBlog.App.Platforms.iOS.Handlers;

public class CustomShellItemTransition : IShellItemTransition
{
    public Task Transition(IShellItemRenderer oldRenderer, IShellItemRenderer newRenderer)
    {
        var anim = PageTransitionExtensions.GetRoot(newRenderer.ShellItem);

        var task = new TaskCompletionSource<bool>();
        var oldView = oldRenderer.ViewController.View;
        var newView = newRenderer.ViewController.View;

        oldView.Layer.RemoveAllAnimations();

        if (anim.AbovePage == TransitionPageType.NextPage)
        {
            oldView.Superview.InsertSubviewAbove(newView, oldView);
        }
        else
        {
            oldView.Superview.InsertSubviewBelow(newView, oldView);
        }

        var actionOut = () =>
        {
            task.TrySetResult(true);
        };
        var actionIn = actionOut;
        
        if (anim.AnimationIn.Duration > anim.AnimationOut.Duration)
        {
            actionOut = default;
        }
        else
        {
            actionIn = default;
        }
        
        PageTransitionExtensions.Animate(oldView, anim.AnimationOut, actionOut);
        PageTransitionExtensions.Animate(newView, anim.AnimationIn, actionIn);

        return task.Task;
    }
}