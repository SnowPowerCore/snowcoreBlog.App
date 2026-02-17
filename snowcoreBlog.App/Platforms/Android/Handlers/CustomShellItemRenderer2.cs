using System.Runtime.CompilerServices;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AndroidX.Core.View;
using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;
using Xamarin.Android.BlurView;
using Xamarin.Android.BlurView.Interfaces;
using Xamarin.Android.BlurView.Renders;
using AColor = Android.Graphics.Color;
using AndroidContent = Android.Content.Res;
using AResource = Android.Resource;
using AView = Android.Views.View;

namespace snowcoreBlog.App.Platforms.Android.Handlers;

public class CustomShellItemRenderer2(IShellContext shellContext) : ShellItemRenderer(shellContext)
{
    private const float OverlayMarginDp = 24f;
    private const float ItemInsetDp = 3.5f;
    private const float ItemCornerRadiusDp = 23f;
    private const float SelectedLightenFactor = 0.15f;
    private const string ItemBackgroundTag = "TabItemBackground";
    private const string ItemContentTag = "TabItemContent";
    private const string ItemBlurTag = "BottomBarBlur";
    private const float MinScaleX = 0.75f;
    private const float MinScaleY = 0.9f;
    private const long SelectDurationMs = 360;
    private const long DeselectDurationMs = 200;
    private WeakReference<BottomNavigationView>? _bottomNavViewRef;
    private static readonly IInterpolator SelectInterpolator = new PathInterpolator(0.2f, 0f, 0f, 1f);
    private static readonly IInterpolator DeselectInterpolator = new PathInterpolator(0.3f, 0f, 0.2f, 1f);
    private readonly ConditionalWeakTable<AView, SelectionState> _selectionStates = [];
    private readonly ConditionalWeakTable<AView, ViewMarginState> _marginStates = [];
    private Drawable.ConstantState? _selectedItemState;
    private int _cachedSelectedArgb;
    private int _cachedInsetPx;
    private float _cachedCornerRadiusPx;

    public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        var root = base.OnCreateView(inflater, container, savedInstanceState);
        if (root is default(AView))
        {
            return new FrameLayout(container?.Context ?? inflater.Context!);
        }

        if (root is ViewGroup rootGroup)
        {
            TryApplyOverlayLayout(rootGroup);
        }

        var bottomNavigationView = FindBottomNavigationView(root);
        if (bottomNavigationView is not default(BottomNavigationView))
        {
            _bottomNavViewRef = new WeakReference<BottomNavigationView>(bottomNavigationView);
            ApplyBottomBarInsets(bottomNavigationView);
        }

        return root;
    }

    public override void OnConfigurationChanged(Configuration newConfig)
    {
        base.OnConfigurationChanged(newConfig);
        ReapplyInsets();
    }

    private void ReapplyInsets()
    {
        if (_bottomNavViewRef?.TryGetTarget(out var bottomNav) is true)
        {
            bottomNav.Post(() => ViewCompat.RequestApplyInsets(bottomNav));
        }
    }

    private void ApplyBottomBarInsets(BottomNavigationView bottomNavigationView)
    {
        ViewCompat.SetOnApplyWindowInsetsListener(bottomNavigationView, new BottomNavigationInsetsListener(_marginStates));
        bottomNavigationView.Post(() => ViewCompat.RequestApplyInsets(bottomNavigationView));
    }

    private static BottomNavigationView? FindBottomNavigationView(AView? view)
    {
        if (view == null)
        {
            return null;
        }

        if (view is BottomNavigationView bottomNavigationView)
        {
            return bottomNavigationView;
        }

        if (view is not ViewGroup viewGroup)
        {
            return null;
        }

        for (var index = 0; index < viewGroup.ChildCount; index++)
        {
            var child = viewGroup.GetChildAt(index);
            var match = FindBottomNavigationView(child);
            if (match != null)
            {
                return match;
            }
        }

        return null;
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

        var metrics = root.Context.Resources?.DisplayMetrics;
        var overlayMarginPx = metrics == null
            ? 0
            : (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, OverlayMarginDp, metrics);

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

        var marginPx = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, OverlayMarginDp, metrics);
        var cornerRadiusPx = TypedValue.ApplyDimension(ComplexUnitType.Dip, ItemCornerRadiusDp + 5f, metrics);

        var blurView = new BlurView(root.Context)
        {
            DuplicateParentStateEnabled = true,
            Tag = ItemBlurTag,
            OutlineProvider = new RoundedOutlineProvider(cornerRadiusPx),
            ClipToOutline = true
        };

        IBlurAlgorithm blurAlgorithm = Build.VERSION.SdkInt >= BuildVersionCodes.S
            ? new RenderEffectBlur()
            : new RenderScriptBlur(root.Context);

        var overlayColor = Colors.White.WithAlpha(0.09f);

        blurView.SetupWith(navigationArea, blurAlgorithm)
            .SetOverlayColor(AColor.Argb((int)(overlayColor.Alpha * 255), 255, 255, 255).ToArgb())
            .SetBlurRadius(20f);

        // Compute desired width so blur matches and stays centered with the bottom navigation.
        var desiredWidth = ComputeDesiredWidth(metrics, overlayMarginPx);
        var blurLp = CreateCenteredLayoutParams(desiredWidth, overlayMarginPx);
        overlay.AddView(blurView, blurLp);

        // After the bottom view is measured, set the blur height to match it exactly
        bottomView.Post(() =>
        {
            var height = bottomView.Height > 0 ? bottomView.Height : bottomView.MeasuredHeight;
            if (height > 0)
            {
                var currentLp = blurView.LayoutParameters as FrameLayout.LayoutParams ?? blurLp;
                currentLp.Height = height;
                currentLp.Gravity = GravityFlags.Bottom | GravityFlags.CenterHorizontal;
                blurView.LayoutParameters = currentLp;
                ViewCompat.SetOnApplyWindowInsetsListener(blurView,
                    new BottomNavigationInsetsListener(_marginStates));
                blurView.Post(() => ViewCompat.RequestApplyInsets(blurView));
                blurView.RequestLayout();
            }
        });

        // Limit bottom navigation width on large screens (reuse computed desiredWidth).
        var bottomLayoutParams = CreateCenteredLayoutParams(desiredWidth, overlayMarginPx);
        overlay.AddView(bottomView, bottomLayoutParams);
        ApplyItemWrapViews(bottomView);

        root.AddView(overlay);
    }

    private static int ComputeDesiredWidth(DisplayMetrics? metrics, int overlayMarginPx)
    {
        var maxWidthDp = 600f; // maximum width for the bottom bar in dp
        var maxWidthPx = metrics is default(DisplayMetrics)
            ? ViewGroup.LayoutParams.MatchParent
            : (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, maxWidthDp, metrics);

        var screenWidth = metrics?.WidthPixels ?? int.MaxValue;
        var desiredWidth = screenWidth is int.MaxValue
            ? ViewGroup.LayoutParams.MatchParent
            : Math.Min(screenWidth - (overlayMarginPx * 2), maxWidthPx);

        return desiredWidth;
    }

    private static FrameLayout.LayoutParams CreateCenteredLayoutParams(int desiredWidth, int overlayMarginPx)
    {
        return new FrameLayout.LayoutParams(
            desiredWidth,
            ViewGroup.LayoutParams.WrapContent,
            GravityFlags.Bottom | GravityFlags.CenterHorizontal)
        {
            LeftMargin = overlayMarginPx * 2,
            TopMargin = overlayMarginPx / 2,
            RightMargin = overlayMarginPx * 2,
            BottomMargin = overlayMarginPx / 2
        };
    }

    private void EnsureDrawableCache(DisplayMetrics metrics)
    {
        var insetPx = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, ItemInsetDp, metrics);
        var cornerRadiusPx = TypedValue.ApplyDimension(ComplexUnitType.Dip, ItemCornerRadiusDp, metrics);
        var selectedColor = GetSelectedItemBackground();
        var selectedArgb = selectedColor.ToArgb();

        if (_selectedItemState != null
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
        _cachedInsetPx = insetPx;
        _cachedCornerRadiusPx = cornerRadiusPx;
        _cachedSelectedArgb = selectedArgb;
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

            var layoutParams = new FrameLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent)
            {
                LeftMargin = _cachedInsetPx,
                TopMargin = _cachedInsetPx,
                RightMargin = _cachedInsetPx,
                BottomMargin = _cachedInsetPx
            };

            var backgroundItemView = new ImageView(context)
            {
                DuplicateParentStateEnabled = true,
                Tag = ItemBackgroundTag
            };
            backgroundItemView.SetImageDrawable(selectedDrawable);
            backgroundItemView.Alpha = 1f;
            itemView.AddView(backgroundItemView, 0, layoutParams);

            var contentView = CreateCustomItemContent(bottomView, i, metrics);
            if (contentView != null)
            {
                itemView.AddView(contentView);
            }

            // Create and register selection state so we can control initial visibility
            InitSelectionState(itemView, backgroundItemView, bottomView.Menu.GetItem(i)?.IsChecked == true);
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
        if (menuItem == null)
        {
            return null;
        }
        var iconSizePx = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 24f, metrics);
        var labelMarginPx = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 2f, metrics);

        var container = new LinearLayout(context)
        {
            Orientation = global::Android.Widget.Orientation.Vertical,
            Tag = ItemContentTag
        };
        container.SetGravity(GravityFlags.Center);
        container.DuplicateParentStateEnabled = true;
        container.LayoutParameters = new FrameLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent,
            ViewGroup.LayoutParams.MatchParent);

        var iconView = new ImageView(context);
        var iconDrawable = menuItem.Icon;
        if (iconDrawable != null)
            iconView.SetImageDrawable(iconDrawable);
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

    private void InitSelectionState(ViewGroup itemView, ImageView backgroundItemView, bool currentlySelected)
    {
        var state = new SelectionState(itemView, backgroundItemView);
        _selectionStates.Add(itemView, state);

        var isChecked = currentlySelected;
        state.IsSelected = isChecked;
        SetSelectionScale(backgroundItemView, isChecked);

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
                AnimateAlpha(state.BackgroundView, isSelected);
            }
            else
            {
                SetSelectionScale(state.BackgroundView, isSelected);
                SetSelectionAlpha(state.BackgroundView, isSelected);
            }
        }
        else if (lastSelected == null)
        {
            SetSelectionScale(state.BackgroundView, isSelected);
        }
    }

    private static void AnimateSelection(AView backgroundView, bool isSelected)
    {
        var targetScaleX = isSelected ? 1f : MinScaleX;
        var targetScaleY = isSelected ? 1f : MinScaleY;
        var duration = isSelected ? SelectDurationMs : DeselectDurationMs;
        var interpolator = isSelected ? SelectInterpolator : DeselectInterpolator;

        var animator = backgroundView.Animate();
        if (animator == null)
            return;

        animator.Cancel();

        if (isSelected)
        {
            // start from small & invisible
            backgroundView.ScaleX = MinScaleX;
            backgroundView.ScaleY = MinScaleY;
            backgroundView.Alpha = 0f;
        }

        animator.WithLayer()
            .ScaleX(targetScaleX)
            .ScaleY(targetScaleY)
            .Alpha(isSelected ? 1f : 0f)
            .SetDuration(duration)
            .SetInterpolator(interpolator)
            .SetListener(new AnimationEndListener(() =>
            {
                SetSelectionScale(backgroundView, isSelected);
            }));
    }

    private static void AnimateAlpha(AView backgroundView, bool isSelected)
    {
        var targetAlpha = isSelected ? 1f : 0f;
        var duration = isSelected ? SelectDurationMs : DeselectDurationMs;
        var interpolator = isSelected ? SelectInterpolator : DeselectInterpolator;
        
        var animator = backgroundView.Animate();
        if (animator == null)
            return;

        animator?.WithLayer()?
            .Alpha(targetAlpha)
            .SetDuration(duration)
            .SetInterpolator(interpolator)
            .SetListener(new AnimationEndListener(() =>
            {
                SetSelectionAlpha(backgroundView, isSelected);
            }));
    }

    private static void SetSelectionScale(AView backgroundView, bool isSelected)
    {
        if (isSelected)
        {
            backgroundView.ScaleX = 1f;
            backgroundView.ScaleY = 1f;
            backgroundView.Alpha = 1f;
        }
        else
        {
            backgroundView.ScaleX = MinScaleX;
            backgroundView.ScaleY = MinScaleY;
            backgroundView.Alpha = 0f;
        }
    }

    private static void SetSelectionAlpha(AView backgroundView, bool isSelected)
    {
        backgroundView.Alpha = isSelected ? 1f : 0f;
    }

    private sealed class AnimationEndListener : Java.Lang.Object, global::Android.Animation.Animator.IAnimatorListener
    {
        private readonly Action _onEnd;

        public AnimationEndListener(Action onEnd) => _onEnd = onEnd;

        public void OnAnimationCancel(global::Android.Animation.Animator animation) { }
        public void OnAnimationEnd(global::Android.Animation.Animator animation) => _onEnd?.Invoke();
        public void OnAnimationRepeat(global::Android.Animation.Animator animation) { }
        public void OnAnimationStart(global::Android.Animation.Animator animation) { }
    }

    private sealed class SelectionLayoutListener(SelectionState state) : Java.Lang.Object, AView.IOnLayoutChangeListener
    {
        private readonly SelectionState _state = state;

        public void OnLayoutChange(AView? v, int left, int top, int right, int bottom,
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

    private sealed class RoundedOutlineProvider : ViewOutlineProvider
    {
        private readonly float _radius;

        public RoundedOutlineProvider(float radius) => _radius = radius;

        public override void GetOutline(AView? view, Outline? outline)
        {
            if (view == null || outline == null)
            {
                return;
            }

            var width = view.Width;
            var height = view.Height;
            if (width <= 0 || height <= 0)
            {
                outline.SetEmpty();
                return;
            }

            outline.SetRoundRect(0, 0, width, height, _radius);
        }
    }

    private AColor GetSelectedItemBackground()
    {
        var tabBarColor = MauiControls.Shell.GetTabBarBackgroundColor(ShellItem);
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

    private static ColorStateList GetLabelColorStateList()
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

        return new ColorStateList(states, colors);
    }

    private sealed class BottomNavigationInsetsListener(ConditionalWeakTable<AView, ViewMarginState> marginStates)
        : Java.Lang.Object, IOnApplyWindowInsetsListener
    {
    	private readonly int _systemBarsInsetsType = WindowInsets.Type.SystemBars();
    	private readonly int _displayCutoutInsetsType = WindowInsets.Type.DisplayCutout();
        private readonly ConditionalWeakTable<AView, ViewMarginState> _marginStates = marginStates;

        public WindowInsetsCompat? OnApplyWindowInsets(AView? v, WindowInsetsCompat? insets)
        {
            if (v == null || insets == null)
            {
                return insets;
            }

            if (v.LayoutParameters is not ViewGroup.MarginLayoutParams marginLayoutParams)
            {
                return insets;
            }

            var state = _marginStates.GetOrCreateValue(v);
            if (!state.IsCaptured)
            {
                state.Capture(marginLayoutParams);
            }

            var mergedInsets = ResolveSystemAndCutoutInsets(v, insets);
            // In horizontal (landscape) orientation we want the bottom bar to stay centered
            // and not pick up large left/right insets, so ignore left/right insets there.
            try
            {
                var cfg = v.Context?.Resources?.Configuration;
                if (cfg != null && cfg.Orientation == AndroidContent.Orientation.Landscape)
                {
                    mergedInsets = new ResolvedInsets(0, mergedInsets.Top, 0, mergedInsets.Bottom);
                }
            }
            catch
            {
                // Fall back to applying full insets on any error retrieving configuration.
            }
            marginLayoutParams.SetMargins(
                state.Left + mergedInsets.Left,
                state.Top,
                state.Right + mergedInsets.Right,
                state.Bottom + mergedInsets.Bottom);
            v.LayoutParameters = marginLayoutParams;

			v.RequestLayout();
			v.Invalidate();

            return insets;
        }

        private ResolvedInsets ResolveSystemAndCutoutInsets(AView? view, WindowInsetsCompat? insets)
        {
            var sourceInsets = view == null ? insets : ViewCompat.GetRootWindowInsets(view) ?? insets;
            if (sourceInsets == null)
            {
                return new ResolvedInsets(0, 0, 0, 0);
            }

            // Child listeners often receive consumed/zero values; this API keeps stable bar sizes.
            var systemBars = sourceInsets.GetInsetsIgnoringVisibility(_systemBarsInsetsType);
            var displayCutout = sourceInsets.GetInsets(_displayCutoutInsetsType);

            var systemLeft = systemBars?.Left ?? 0;
            var systemTop = systemBars?.Top ?? 0;
            var systemRight = systemBars?.Right ?? 0;
            var systemBottom = systemBars?.Bottom ?? 0;

            var cutoutLeft = displayCutout?.Left ?? 0;
            var cutoutTop = displayCutout?.Top ?? 0;
            var cutoutRight = displayCutout?.Right ?? 0;
            var cutoutBottom = displayCutout?.Bottom ?? 0;

            return new ResolvedInsets(
                Math.Max(systemLeft, cutoutLeft),
                Math.Max(systemTop, cutoutTop),
                Math.Max(systemRight, cutoutRight),
                Math.Max(systemBottom, cutoutBottom));
        }

        private readonly record struct ResolvedInsets(int Left, int Top, int Right, int Bottom);
    }

    private sealed class ViewMarginState
    {
        public bool IsCaptured { get; private set; }
        public int Left { get; private set; }
        public int Top { get; private set; }
        public int Right { get; private set; }
        public int Bottom { get; private set; }

        public void Capture(ViewGroup.MarginLayoutParams layoutParams)
        {
            Left = layoutParams.LeftMargin;
            Top = layoutParams.TopMargin;
            Right = layoutParams.RightMargin;
            Bottom = layoutParams.BottomMargin;
            IsCaptured = true;
        }
    }
}