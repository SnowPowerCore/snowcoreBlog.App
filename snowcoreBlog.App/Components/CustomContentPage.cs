using Nalu;
using snowcoreBlog.App.Extensions;

namespace snowcoreBlog.App.Components;

public partial class CustomContentPage : Component<CustomContentPageState>
{
    private readonly VisualNode _invisibleContentView = ContentView().IsHeadless();

    [Inject]
    private readonly INavigationService _navigation;

    [Prop]
	private string _title = string.Empty;

    [Prop]
	public Func<VisualNode>? _titleBarView;

    [Prop]
    private Func<IEnumerable<VisualNode>?>? _content;

    [Prop]
    private Func<Task>? _onNavigatedBack;

    [Prop]
	public bool? _isPreviousPageButtonVisible;

    public override VisualNode Render()
    {
        _content ??= () => [_invisibleContentView];
        return ContentPage(
            Grid("Auto, *, 0.15*", string.Empty,
                ContentView(
                    _content!.Invoke()
                )
                    .IsHeadless()
                    .GridRow(1)
                    .GridRowSpan(2),

                ContentView()
                    .Background(new LinearGradientBrush(
                        [
                            new GradientStop(Theme.IsDarkTheme ? ApplicationTheme.NeutralDarkDarkest.WithAlpha(0.6f) : ApplicationTheme.NeutralLightLightest.WithAlpha(0.6f), 1f),
                            new GradientStop(Theme.IsDarkTheme ? ApplicationTheme.NeutralDarkDarkest.WithAlpha(0.6f) : ApplicationTheme.NeutralLightLightest.WithAlpha(0.6f), 0.5f),
                            new GradientStop(Theme.IsDarkTheme ? ApplicationTheme.NeutralDarkDarkest.WithAlpha(0.001f) : ApplicationTheme.NeutralLightLightest.WithAlpha(0.001f), 0f)
                        ],
                        startPoint: new Point(0.5, 0),
                        endPoint: new Point(0.5, 1)))
                    .GridRow(2),

                TopInsetsView(
                    Grid(string.Empty, "0.25*, *, Auto",
                        Label(() => _title)
                            .HorizontalTextAlignment(TextAlignment.Center)
                            .LineBreakMode(LineBreakMode.TailTruncation)
                            .FontFamily("OpenSansSemibold")
                            .FontSize(ApplicationTheme.SizeHeading1)
                            .IsVisible(() => !string.IsNullOrEmpty(_title))
                            .GridColumnSpan(3),

                        ContentView(
                            _titleBarView!.Invoke()
                        )
                            .IsHeadless()
                            .Margin(0, 0, 15, 0)
                            .GridColumn(2),

                        ContentView(
                            SvgCachedImage("icon_back.png")
                                .HeightRequest(24)
                                .Margin(new Thickness(16, 8, 0, 8))
                                .HorizontalOptions(LayoutOptions.Start)
                            )
                            .HeightRequest(32)
                            .IsVisible(_isPreviousPageButtonVisible ?? State.IsBackButtonVisible ?? false)
                            .OnTapped(_onNavigatedBack ?? (() => _navigation.GoToAsync(Nalu.Navigation.Relative().Pop())))
                    )
                    .IsHeadless()
                )
            )
            .IsHeadless()
        )
        .OnAppearing(OnCustomPageAppearingAsync)
        .ShellNavBarIsVisible(false)
        .EdgeToEdge();
    }

    private Task OnCustomPageAppearingAsync()
    {
        SetupTitle();
        return Task.CompletedTask;
    }

    private void SetupTitle()
    {
        if (_isPreviousPageButtonVisible is not null)
            return;

        Application.Current?.Dispatcher.Dispatch(() =>
            SetState(_ => _.IsBackButtonVisible = Navigation.NavigationStack.Count > 1));
    }
}