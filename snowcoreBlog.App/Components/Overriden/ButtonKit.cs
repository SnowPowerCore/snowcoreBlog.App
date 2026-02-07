namespace snowcoreBlog.App.Components.Overriden;

public partial class ButtonKit : Component
{
    [Prop]
    private string? _text;

    [Prop]
    private Func<string>? _textFunc;

    [Prop]
    private LayoutOptions _horizontalOptions = LayoutOptions.Center;

    [Prop]
    private LayoutOptions _verticalOptions = LayoutOptions.Center;

    [Prop]
    private Func<Task>? _onClicked;

    [Prop]
    private string? _leftImageSource;

    [Prop]
    private Func<VisualNode>? _leftImage;

    [Prop]
    private string? _rightImageSource;

    [Prop]
    private Func<VisualNode>? _rightImage;

    public override VisualNode Render()
    {
        bool isPrimary = ThemeKey == ApplicationTheme.Primary;

        return Grid(
            Button()
                .ThemeKey(ThemeKey)
                .OnClicked(_onClicked),

            HStack(spacing: 8,

                _leftImage?.Invoke(),

                _leftImageSource == null ? null :
                Image(_leftImageSource)
                    .VCenter(),

                _textFunc is not default(Func<string>)
                    ? Label(_textFunc)
                        .TextColor(isPrimary ? ApplicationTheme.NeutralLightLightest : ApplicationTheme.HighlightDarkest)
                        .FontSize(ApplicationTheme.SizeCaptionM)
                        .HorizontalTextAlignment(TextAlignment.Center)
                        .VerticalTextAlignment(TextAlignment.Center)
                    : Label(_text)
                        .TextColor(isPrimary ? ApplicationTheme.NeutralLightLightest : ApplicationTheme.HighlightDarkest)
                        .FontSize(ApplicationTheme.SizeCaptionM)
                        .HorizontalTextAlignment(TextAlignment.Center)
                        .VerticalTextAlignment(TextAlignment.Center),

                _rightImage?.Invoke(),

                _rightImageSource == null ? null :
                Image(_rightImageSource)
                    .VCenter()
            )
            .Margin(16, 0)
            .OnTapped(_onClicked)
            .HCenter()
        )
        .HorizontalOptions(_horizontalOptions)
        .VerticalOptions(_verticalOptions);
    }
}