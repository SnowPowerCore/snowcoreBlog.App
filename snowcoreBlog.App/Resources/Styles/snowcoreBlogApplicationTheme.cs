namespace snowcoreBlog.App.Resources.Styles;

public class snowcoreBlogApplicationTheme : InternalCustomApplicationTheme
{
    protected override void OnApply()
    {
        LabelStyles.Default = static _ =>
        {
            LabelExtensions.TextColor(
                LabelExtensions.FontFamily(_, "InterRegular"),
                IsDarkTheme ? NeutralLightLightest : NeutralDarkDarkest);
        };
        LabelStyles.Themes[H1] = static _ =>
        {
            LabelExtensions.FontSize(
                LabelExtensions.TextColor(LabelExtensions.FontFamily(_, "InterExtraBold"),
                IsDarkTheme ? NeutralLightLightest : NeutralDarkDarkest), SizeHeading1);
        };
        LabelStyles.Themes[H2] = static _ =>
        {
            LabelExtensions.FontSize(
                LabelExtensions.TextColor(LabelExtensions.FontFamily(_, "InterExtraBold"),
                IsDarkTheme ? NeutralLightLightest : NeutralDarkDarkest), SizeHeading2);
        };
        LabelStyles.Themes[H3] = static _ =>
        {
            LabelExtensions.FontSize(
                LabelExtensions.TextColor(LabelExtensions.FontFamily(_, "InterExtraBold"),
                IsDarkTheme ? NeutralLightLightest : NeutralDarkDarkest), SizeHeading3);
        };
        LabelStyles.Themes[H4] = static _ =>
        {
            LabelExtensions.FontSize(
                LabelExtensions.TextColor(LabelExtensions.FontFamily(_, "InterBold"),
                IsDarkTheme ? NeutralLightLightest : NeutralDarkDarkest), SizeHeading4);
        };
        LabelStyles.Themes[H4] = static _ =>
        {
            LabelExtensions.FontSize(
                LabelExtensions.TextColor(LabelExtensions.FontFamily(_, "InterBold"),
                IsDarkTheme ? NeutralLightLightest : NeutralDarkDarkest), SizeHeading4);
        };
        LabelStyles.Themes[H5] = static _ =>
        {
            LabelExtensions.FontSize(
                LabelExtensions.TextColor(LabelExtensions.FontFamily(_, "InterBold"),
                IsDarkTheme ? NeutralLightLightest : NeutralDarkDarkest), SizeHeading5);
        };
        LabelStyles.Themes[BodyXL] = static _ =>
        {
            LabelExtensions.FontSize(
                LabelExtensions.TextColor(LabelExtensions.FontFamily(_, "InterRegular"),
                IsDarkTheme ? NeutralLightLightest : NeutralDarkDarkest), SizeBodyXL);
        };
        LabelStyles.Themes[BodyL] = static _ =>
        {
            LabelExtensions.FontSize(
                LabelExtensions.FontFamily(_, "InterRegular"), SizeBodyL);
        };
        LabelStyles.Themes[BodyM] = static _ =>
        {
            LabelExtensions.FontSize(
                LabelExtensions.TextColor(LabelExtensions.FontFamily(_, "InterRegular"),
                IsDarkTheme ? NeutralLightLightest : NeutralDarkDarkest), SizeBodyM);
        };
        LabelStyles.Themes[BodyS] = static _ =>
        {
            LabelExtensions.FontSize(
                LabelExtensions.TextColor(LabelExtensions.FontFamily(_, "InterRegular"),
                IsDarkTheme ? NeutralLightLightest : NeutralDarkDarkest), SizeBodyS);
        };
        LabelStyles.Themes[BodyXS] = static _ =>
        {
            LabelExtensions.FontSize(
                LabelExtensions.FontFamily(_, "InterMedium"), SizeBodyXS);
        };
        LabelStyles.Themes[ActionL] = static _ =>
        {
            LabelExtensions.FontSize(
                LabelExtensions.TextColor(LabelExtensions.FontFamily(_, "InterSemiBold"),
                IsDarkTheme ? NeutralLightLightest : NeutralDarkDarkest), SizeActionL);
        };
        LabelStyles.Themes[ActionM] = static _ =>
        {
            LabelExtensions.FontSize(
                LabelExtensions.TextColor(LabelExtensions.FontFamily(_, "InterSemiBold"),
                IsDarkTheme ? NeutralLightLightest : NeutralDarkDarkest), SizeActionM);
        };
        LabelStyles.Themes[ActionS] = static _ =>
        {
            LabelExtensions.FontSize(
                LabelExtensions.TextColor(LabelExtensions.FontFamily(_, "InterSemiBold"),
                IsDarkTheme ? NeutralLightLightest : NeutralDarkDarkest), SizeActionS);
        };
        LabelStyles.Themes[CaptionM] = static _ =>
        {
            LabelExtensions.FontSize(
                LabelExtensions.TextColor(LabelExtensions.FontFamily(_, "InterSemiBold"),
                IsDarkTheme ? NeutralLightLightest : NeutralDarkDarkest), SizeCaptionM);
        };
        ButtonStyles.Default = static _ =>
        {
            ButtonExtensions.Padding(ButtonExtensions.CornerRadius(ButtonExtensions.FontSize(VisualElementExtensions.BackgroundColor(ButtonExtensions.TextColor(ButtonExtensions.FontFamily(_, "InterSemiBold"), IsDarkTheme ? NeutralDarkDarkest : NeutralLightLightest), HighlightDarkest), SizeCaptionM), 12), 16.0, 12.5).Height(40.0);
        };
        ButtonStyles.Themes[Primary] = static _ =>
        {
            ButtonExtensions.Padding(ButtonExtensions.CornerRadius(ButtonExtensions.FontSize(VisualElementExtensions.BackgroundColor(ButtonExtensions.TextColor(ButtonExtensions.FontFamily(_, "InterSemiBold"), IsDarkTheme ? NeutralDarkDarkest : NeutralLightLightest), HighlightDarkest), SizeCaptionM), 12), 16.0, 12.5).Height(40.0);
        };
        ButtonStyles.Themes[Secondary] = static _ =>
        {
            ButtonExtensions.Padding(ButtonExtensions.CornerRadius(ButtonExtensions.FontSize(ButtonExtensions.BorderWidth(ButtonExtensions.BorderColor(VisualElementExtensions.BackgroundColor(ButtonExtensions.TextColor(ButtonExtensions.FontFamily(_, "InterSemiBold"), HighlightDarkest), IsDarkTheme ? NeutralDarkDarkest : NeutralLightLightest), HighlightDarkest), 1.0), SizeCaptionM), 12), 16.0, 12.5).Height(40.0);
        };
        ButtonStyles.Themes[Terciary] = static _ =>
        {
            ButtonExtensions.Padding(ButtonExtensions.CornerRadius(ButtonExtensions.FontSize(ButtonExtensions.BorderWidth(VisualElementExtensions.BackgroundColor(ButtonExtensions.TextColor(ButtonExtensions.FontFamily(_, "InterSemiBold"), HighlightDarkest), IsDarkTheme ? NeutralDarkDarkest : NeutralLightLightest), 0.0), SizeCaptionM), 12), 16.0, 12.5).Height(40.0);
        };
        EntryStyles.Default = static _ =>
        {
            InputViewExtensions.FontSize(VisualElementExtensions.BackgroundColor(InputViewExtensions.TextColor(InputViewExtensions.FontFamily(_, "InterRegular"), NeutralDarkDarkest), IsDarkTheme ? NeutralLightLightest : NeutralDarkDarkest), SizeBodyM);
        };
        ContentPageStyles.Default = static _ =>
        {
            VisualElementExtensions.BackgroundColor(_, IsDarkTheme ? NeutralDarkDarkest : NeutralLightLightest);
        };
    }
}