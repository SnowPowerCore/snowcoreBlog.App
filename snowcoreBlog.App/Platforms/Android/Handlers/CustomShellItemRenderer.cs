using Android.Graphics.Drawables;
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
using AndroidContent = Android.Content;

namespace snowcoreBlog.App.Platforms.Android.Handlers;

public class CustomShellItemRenderer : ShellItemRenderer
{
    private const float InitialMarginDp = 12f;
    private const float ItemInsetDp = 2.6f;
    private const float ItemCornerRadiusDp = 15f;
    private const float SelectedLightenFactor = 0.15f;
    private const float MinScale = 0.75f;
    private const long SelectDurationMs = 120;
    private const long DeselectDurationMs = 120;
    private const string ItemBackgroundTag = "TabItemBackground";
    private const string ItemContentTag = "TabItemContent";
    private readonly ConditionalWeakTable<AView, SelectionState> _selectionStates = new();
    private Drawable.ConstantState? _selectedItemState;
    private Drawable.ConstantState? _unselectedItemState;
    private int _cachedSelectedArgb;
    private int _cachedInsetPx;
    private float _cachedCornerRadiusPx;

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

        EnsureDrawableCache(metrics);

        for (var i = 0; i < menuView.ChildCount; i++)
        {
            if (menuView.GetChildAt(i) is not ViewGroup itemView)
            {
                continue;
            }

            if (itemView.FindViewWithTag(ItemBackgroundTag) is not null
                && itemView.FindViewWithTag(ItemContentTag) is not null)
            {
                continue;
            }

            itemView.RemoveAllViews();

            var selectedDrawable = _selectedItemState?.NewDrawable()?.Mutate() as GradientDrawable
                ?? new GradientDrawable();
            var unselectedDrawable = _unselectedItemState?.NewDrawable()?.Mutate() as GradientDrawable
                ?? new GradientDrawable();

            var stateList = new StateListDrawable();
            stateList.AddState([AResource.Attribute.StateChecked], new InsetDrawable(selectedDrawable, _cachedInsetPx));
            stateList.AddState([-AResource.Attribute.StateChecked], new InsetDrawable(unselectedDrawable, _cachedInsetPx));

            var backgroundView = new AView(context)
            {
                DuplicateParentStateEnabled = true,
                Tag = ItemBackgroundTag,
                Background = stateList
            };

            var layoutParams = new FrameLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent)
            {
                LeftMargin = _cachedInsetPx,
                TopMargin = _cachedInsetPx,
                RightMargin = _cachedInsetPx,
                BottomMargin = _cachedInsetPx
            };

            itemView.SetClipToPadding(false);
            itemView.SetClipChildren(false);
            itemView.AddView(backgroundView, 0, layoutParams);

            var contentView = CreateCustomItemContent(bottomView, i, metrics);
            if (contentView != null)
            {
                itemView.AddView(contentView);
            }

            EnsureSelectionAnimator(itemView, backgroundView);
        }
    }

    private static LinearLayout? CreateCustomItemContent(BottomNavigationView bottomView, int index, DisplayMetrics metrics)
    {
        var context = bottomView.Context;
        if (context == null)
        {
            return null;
        }

        var menu = bottomView.Menu;
        if (menu == null || index < 0 || index >= menu.Size())
        {
            return null;
        }

        var menuItem = menu.GetItem(index);
        var iconSizePx = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 24f, metrics);
        var labelMarginPx = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 2f, metrics);

        var container = new LinearLayout(context)
        {
            Orientation = Orientation.Vertical,
            Tag = ItemContentTag
        };
        container.SetGravity(GravityFlags.Center);
        container.DuplicateParentStateEnabled = true;
        container.LayoutParameters = new FrameLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent,
            ViewGroup.LayoutParams.MatchParent);

        var iconView = new ImageView(context);
        iconView.SetImageDrawable(menuItem.Icon);
        iconView.DuplicateParentStateEnabled = true;
        iconView.ImageTintList = bottomView.ItemIconTintList;
        iconView.LayoutParameters = new LinearLayout.LayoutParams(iconSizePx, iconSizePx)
        {
            Gravity = GravityFlags.CenterHorizontal
        };

        var labelView = new TextView(context)
        {
            Text = menuItem.TitleFormatted?.ToString() ?? menuItem.TitleCondensedFormatted?.ToString() ?? string.Empty,
            DuplicateParentStateEnabled = true
        };
        labelView.SetTextColor(GetLabelColorStateList());
        labelView.SetTextSize(ComplexUnitType.Sp, 12f);
        labelView.LayoutParameters = new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.WrapContent,
            ViewGroup.LayoutParams.WrapContent)
        {
            Gravity = GravityFlags.CenterHorizontal,
            TopMargin = labelMarginPx
        };

        container.AddView(iconView);
        container.AddView(labelView);

        return container;
    }

    private void EnsureDrawableCache(DisplayMetrics metrics)
    {
        var insetPx = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, ItemInsetDp, metrics);
        var cornerRadiusPx = TypedValue.ApplyDimension(ComplexUnitType.Dip, ItemCornerRadiusDp, metrics);
        var selectedColor = GetSelectedItemBackground();
        var selectedArgb = selectedColor.ToArgb();

        if (_selectedItemState != null
            && _unselectedItemState != null
            && _cachedInsetPx == insetPx
            && Math.Abs(_cachedCornerRadiusPx - cornerRadiusPx) < 0.5f
            && _cachedSelectedArgb == selectedArgb)
        {
            return;
        }

        var selectedDrawable = new GradientDrawable();
        selectedDrawable.SetColor(selectedColor);
        selectedDrawable.SetCornerRadius(cornerRadiusPx);

        var unselectedDrawable = new GradientDrawable();
        unselectedDrawable.SetColor(AColor.Transparent);
        unselectedDrawable.SetCornerRadius(cornerRadiusPx);

        _selectedItemState = selectedDrawable.GetConstantState();
        _unselectedItemState = unselectedDrawable.GetConstantState();
        _cachedInsetPx = insetPx;
        _cachedCornerRadiusPx = cornerRadiusPx;
        _cachedSelectedArgb = selectedArgb;
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
        backgroundView.Alpha = isSelected ? 1f : 0f;
        if (isSelected)
        {
            backgroundView.ScaleX = MinScale;
            backgroundView.ScaleY = MinScale;
        }

        animator
            .WithLayer()
            .ScaleX(targetScale)
            .ScaleY(targetScale)
            .SetDuration(duration)
            .SetInterpolator(interpolator)
            .SetListener(null);
    }

    private static void SetSelectionScale(AView backgroundView, bool isSelected)
    {
        backgroundView.ScaleX = 1f;
        backgroundView.ScaleY = 1f;
        backgroundView.Alpha = isSelected ? 1f : 0f;
    }

    private sealed class SelectionLayoutListener(SelectionState state) : Java.Lang.Object, AView.IOnLayoutChangeListener
    {
        private readonly SelectionState _state = state;

        public void OnLayoutChange(AView v, int left, int top, int right, int bottom,
            int oldLeft, int oldTop, int oldRight, int oldBottom)
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

    private sealed class SelectionState(AView itemView, AView backgroundView)
    {
        public AView ItemView { get; } = itemView;
        public AView BackgroundView { get; } = backgroundView;
        public bool? IsSelected { get; set; }
        public SelectionLayoutListener? Listener { get; set; }
    }

    private AColor GetSelectedItemBackground()
    {
        var tabBarColor = Microsoft.Maui.Controls.Shell.GetTabBarBackgroundColor(ShellItem);
        var baseColor = tabBarColor ?? ShellRenderer.DefaultBackgroundColor;
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

    private static AndroidContent.Res.ColorStateList GetLabelColorStateList()
    {
        var selected = AColor.Argb(255, 255, 255, 255).ToArgb();
        var unselected = AColor.Argb(160, 255, 255, 255).ToArgb();

        var states = new int[][]
        {
            [-AResource.Attribute.StateEnabled],
            [AResource.Attribute.StateChecked],
            []
        };

        var colors = new[] { unselected, selected, unselected };

        return new AndroidContent.Res.ColorStateList(states, colors);
    }
}