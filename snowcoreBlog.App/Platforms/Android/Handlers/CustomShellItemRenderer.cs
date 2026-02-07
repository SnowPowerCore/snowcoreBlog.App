using Android.Graphics.Drawables;
using Android.Animation;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using System.Runtime.CompilerServices;
using Android.Widget;
using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;
using AColor = Android.Graphics.Color;
using AResource = Android.Resource;
using AView = Android.Views.View;

namespace snowcoreBlog.App.Platforms.Android.Handlers;

public class CustomShellItemRenderer : ShellItemRenderer
{
    private const float InitialMarginDp = 12f;
    private const float ItemInsetDp = 2.6f;
    private const float ItemCornerRadiusDp = 15f;
    private const float SelectedLightenFactor = 0.15f;
    private const float MinScale = 0.95f;
    private const long SelectDurationMs = 180;
    private const long DeselectDurationMs = 160;
    private const string ItemBackgroundTag = "TabItemBackground";
    private readonly ConditionalWeakTable<AView, SelectionState> _selectionStates = new();

    public CustomShellItemRenderer(IShellContext shellContext) : base(shellContext) { }

    public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        var root = base.OnCreateView(inflater, container, savedInstanceState);
        if (root == null)
        {
            return new FrameLayout(container?.Context ?? inflater.Context);
        }

        if (root is ViewGroup rootGroup)
        {
            TryApplyOverlayLayout(rootGroup);
        }

        return root;
    }

    private void TryApplyOverlayLayout(ViewGroup root)
    {
        BottomNavigationView? bottomView = null;
        ViewGroup? navigationArea = null;

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

        if (root.Context == null)
        {
            return;
        }

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
        ApplyItemWrapViews(bottomView);
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
            marginLayoutParams.SetMargins(marginPx + marginPx, marginPx, marginPx + marginPx, marginPx);
            bottomView.LayoutParameters = marginLayoutParams;
            bottomView.RequestLayout();
        }
    }

    private void ApplyItemWrapViews(BottomNavigationView bottomView)
    {
        var context = bottomView.Context;
        if (context == null)
        {
            return;
        }

        if (bottomView.GetChildAt(0) is not ViewGroup menuView)
        {
            bottomView.Post(() => ApplyItemWrapViews(bottomView));
            return;
        }

        var metrics = context.Resources?.DisplayMetrics
            ?? context.ApplicationContext?.Resources?.DisplayMetrics;

        if (metrics == null)
        {
            return;
        }

        var insetPx = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, ItemInsetDp, metrics);
        var cornerRadiusPx = TypedValue.ApplyDimension(ComplexUnitType.Dip, ItemCornerRadiusDp, metrics);

        for (var i = 0; i < menuView.ChildCount; i++)
        {
            if (menuView.GetChildAt(i) is not ViewGroup itemView)
            {
                continue;
            }

            if (itemView.FindViewWithTag(ItemBackgroundTag) is AView)
            {
                continue;
            }

            var selectedDrawable = new GradientDrawable();
            selectedDrawable.SetColor(GetSelectedItemBackground());
            selectedDrawable.SetCornerRadius(cornerRadiusPx);

            var unselectedDrawable = new GradientDrawable();
            unselectedDrawable.SetColor(AColor.Transparent);
            unselectedDrawable.SetCornerRadius(cornerRadiusPx);

            var stateList = new StateListDrawable();
            stateList.AddState(new[] { AResource.Attribute.StateChecked }, new InsetDrawable(selectedDrawable, insetPx));
            stateList.AddState(new[] { -AResource.Attribute.StateChecked }, new InsetDrawable(unselectedDrawable, insetPx));

            var backgroundView = new AView(context)
            {
                DuplicateParentStateEnabled = true,
                Tag = ItemBackgroundTag
            };
            backgroundView.Background = stateList;

            var layoutParams = new FrameLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent)
            {
                LeftMargin = insetPx,
                TopMargin = insetPx,
                RightMargin = insetPx,
                BottomMargin = insetPx
            };

            itemView.SetClipToPadding(false);
            itemView.SetClipChildren(false);
            itemView.AddView(backgroundView, 0, layoutParams);

            EnsureSelectionAnimator(itemView, backgroundView);
        }
    }

    private void EnsureSelectionAnimator(ViewGroup itemView, AView backgroundView)
    {
        if (_selectionStates.TryGetValue(itemView, out _))
        {
            return;
        }

        var state = new SelectionState(itemView, backgroundView);
        _selectionStates.Add(itemView, state);

        UpdateSelectionState(state, animate: false);

        var listener = new SelectionLayoutListener(state);
        itemView.AddOnLayoutChangeListener(listener);
        state.Listener = listener;
    }

    private static void UpdateSelectionState(SelectionState state, bool animate)
    {
        var isSelected = state.ItemView.Selected || state.ItemView.Activated;
        var lastSelected = state.IsSelected;

        if (lastSelected == null || lastSelected != isSelected)
        {
            state.IsSelected = isSelected;

            if (animate)
            {
                AnimateSelection(state.BackgroundView, isSelected);
            }
            else
            {
                SetSelectionScale(state.BackgroundView, isSelected);
            }
        }
        else if (lastSelected == null)
        {
            SetSelectionScale(state.BackgroundView, isSelected);
        }
    }

    private static void AnimateSelection(AView backgroundView, bool isSelected)
    {
        var targetScale = isSelected ? 1f : MinScale;
        var duration = isSelected ? SelectDurationMs : DeselectDurationMs;
        IInterpolator interpolator = isSelected
            ? new PathInterpolator(0.2f, 0f, 0.2f, 1f)
            : new PathInterpolator(0.3f, 0f, 0.2f, 1f);

        var animator = backgroundView.Animate();
        if (animator == null)
        {
            return;
        }

        animator.Cancel();
        backgroundView.Alpha = isSelected ? 1f : backgroundView.Alpha;
        if (isSelected)
        {
            backgroundView.ScaleX = MinScale;
            backgroundView.ScaleY = MinScale;
        }

        animator
            .ScaleX(targetScale)
            .ScaleY(targetScale)
            .Alpha(isSelected ? 1f : 0f)
            .SetDuration(duration)
            .SetInterpolator(interpolator)
            .SetListener(isSelected ? null : new ResetScaleOnEndListener(backgroundView));
    }

    private static void SetSelectionScale(AView backgroundView, bool isSelected)
    {
        backgroundView.ScaleX = 1f;
        backgroundView.ScaleY = 1f;
        backgroundView.Alpha = isSelected ? 1f : 0f;
    }

    private sealed class ResetScaleOnEndListener : AnimatorListenerAdapter
    {
        private readonly AView _target;

        public ResetScaleOnEndListener(AView target)
        {
            _target = target;
        }

        public override void OnAnimationEnd(Animator? animation)
        {
            _target.ScaleX = 1f;
            _target.ScaleY = 1f;
        }
    }

    private sealed class SelectionLayoutListener : Java.Lang.Object, AView.IOnLayoutChangeListener
    {
        private readonly SelectionState _state;

        public SelectionLayoutListener(SelectionState state)
        {
            _state = state;
        }

        public void OnLayoutChange(
            AView v,
            int left,
            int top,
            int right,
            int bottom,
            int oldLeft,
            int oldTop,
            int oldRight,
            int oldBottom)
        {
            var width = _state.BackgroundView.Width;
            var height = _state.BackgroundView.Height;
            if (width > 0 && height > 0)
            {
                _state.BackgroundView.PivotX = width / 2f;
                _state.BackgroundView.PivotY = height / 2f;
            }

            UpdateSelectionState(_state, animate: true);
        }
    }

    private sealed class SelectionState
    {
        public SelectionState(AView itemView, AView backgroundView)
        {
            ItemView = itemView;
            BackgroundView = backgroundView;
        }

        public AView ItemView { get; }
        public AView BackgroundView { get; }
        public bool? IsSelected { get; set; }
        public SelectionLayoutListener? Listener { get; set; }
    }

    private AColor GetSelectedItemBackground()
    {
        var tabBarColor = Microsoft.Maui.Controls.Shell.GetTabBarBackgroundColor(ShellItem);
        var baseColor = tabBarColor == null
            ? ShellRenderer.DefaultBackgroundColor
            : tabBarColor;

        var platformColor = baseColor.ToPlatform();
        var tinted = AColor.Argb(220, platformColor.R, platformColor.G, platformColor.B);
        return Lighten(tinted, SelectedLightenFactor);
    }

    private static AColor Lighten(AColor color, float factor)
    {
        var r = color.R + (int)((255 - color.R) * factor);
        var g = color.G + (int)((255 - color.G) * factor);
        var b = color.B + (int)((255 - color.B) * factor);
        return AColor.Argb(color.A, r, g, b);
    }
}
