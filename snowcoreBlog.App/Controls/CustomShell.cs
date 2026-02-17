namespace snowcoreBlog.App.Controls;

public partial class CustomShell : MauiControls.Shell
{
    // Tab bar appearance
    public static readonly BindableProperty TabBarCornerRadiusProperty = BindableProperty.Create(
        nameof(TabBarCornerRadius), typeof(float), typeof(CustomShell), 26f);

    public float TabBarCornerRadius
    {
        get => (float)GetValue(TabBarCornerRadiusProperty);
        set => SetValue(TabBarCornerRadiusProperty, value);
    }

    public static readonly BindableProperty TabBarMarginProperty = BindableProperty.Create(
        nameof(TabBarMargin), typeof(float), typeof(CustomShell), 12f);

    public float TabBarMargin
    {
        get => (float)GetValue(TabBarMarginProperty);
        set => SetValue(TabBarMarginProperty, value);
    }

    public static readonly BindableProperty TabBarElevationProperty = BindableProperty.Create(
        nameof(TabBarElevation), typeof(float), typeof(CustomShell), 4f);

    public float TabBarElevation
    {
        get => (float)GetValue(TabBarElevationProperty);
        set => SetValue(TabBarElevationProperty, value);
    }

    // Blur/backdrop
    public static readonly BindableProperty BottomBarBlurEnabledProperty = BindableProperty.Create(
        nameof(BottomBarBlurEnabled), typeof(bool), typeof(CustomShell), true);

    public bool BottomBarBlurEnabled
    {
        get => (bool)GetValue(BottomBarBlurEnabledProperty);
        set => SetValue(BottomBarBlurEnabledProperty, value);
    }

    public static readonly BindableProperty BottomBarBlurRadiusProperty = BindableProperty.Create(
        nameof(BottomBarBlurRadius), typeof(float), typeof(CustomShell), 20f);

    public float BottomBarBlurRadius
    {
        get => (float)GetValue(BottomBarBlurRadiusProperty);
        set => SetValue(BottomBarBlurRadiusProperty, value);
    }

    public static readonly BindableProperty BottomBarOverlayColorProperty = BindableProperty.Create(
        nameof(BottomBarOverlayColor), typeof(Color), typeof(CustomShell), Colors.White.WithAlpha(0.09f));

    public Color BottomBarOverlayColor
    {
        get => (Color)GetValue(BottomBarOverlayColorProperty);
        set => SetValue(BottomBarOverlayColorProperty, value);
    }

    // Item visuals
    public static readonly BindableProperty ItemCornerRadiusProperty = BindableProperty.Create(
        nameof(ItemCornerRadius), typeof(float), typeof(CustomShell), 23f);

    public float ItemCornerRadius
    {
        get => (float)GetValue(ItemCornerRadiusProperty);
        set => SetValue(ItemCornerRadiusProperty, value);
    }

    public static readonly BindableProperty ItemInsetProperty = BindableProperty.Create(
        nameof(ItemInset), typeof(float), typeof(CustomShell), 3.5f);

    public float ItemInset
    {
        get => (float)GetValue(ItemInsetProperty);
        set => SetValue(ItemInsetProperty, value);
    }

    // Selection tweaks
    public static readonly BindableProperty SelectedLightenFactorProperty = BindableProperty.Create(
        nameof(SelectedLightenFactor), typeof(float), typeof(CustomShell), 0.15f);

    public float SelectedLightenFactor
    {
        get => (float)GetValue(SelectedLightenFactorProperty);
        set => SetValue(SelectedLightenFactorProperty, value);
    }
}
