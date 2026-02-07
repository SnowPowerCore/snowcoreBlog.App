using Microsoft.Extensions.Configuration;
using Nalu;
using snowcoreBlog.App.Components;
using snowcoreBlog.App.Features.Second;

namespace snowcoreBlog.App.Features.Home;

public partial class HomePage : Component<HomePageState, HomePageProps>, IAppearingAware, IDisposable
{
    private bool _disposed = false;

    [Inject]
    private readonly IConfiguration _configuration;

    [Inject]
    private readonly INavigationService _navigation;

    public override VisualNode Render() =>
        ContentPage(
            ScrollView(
                VStack(
                    CachedImage(TranslationResources.DotNetBotImgSrc)
                        .DownsampleToViewSize(true)
                        .HeightRequest(200)
                        .HCenter()
                        .Set(SemanticProperties.DescriptionProperty, TranslationResources.SemanticDotNetBotDesc),

                    Label(_configuration[TranslationResources.AppNameKey])
                        .ThemeKey(ApplicationTheme.H1)
                        .HCenter(),

                    Label(TranslationResources.SubheaderText)
                        .ThemeKey(ApplicationTheme.H3)
                        .HCenter(),

                    ButtonKit(State.Counter == 0
                                ? () => TranslationResources.ButtonStaticText
                                : () => string.Format(TranslationResources.ButtonClickedTimesText, State.Counter))
                        .ThemeKey(ApplicationTheme.Primary)
                        .OnClicked(NavigateToSecondPageAsync),

                    ButtonKit(State.Counter == 0
                                ? () => TranslationResources.ButtonStaticText
                                : () => string.Format(TranslationResources.ButtonClickedTimesText, State.Counter))
                        .ThemeKey(ApplicationTheme.Primary)
                        .OnClicked(NavigateToSecondPageAsync),

                    ButtonKit(State.Counter == 0
                                ? () => TranslationResources.ButtonStaticText
                                : () => string.Format(TranslationResources.ButtonClickedTimesText, State.Counter))
                        .ThemeKey(ApplicationTheme.Primary)
                        .OnClicked(NavigateToSecondPageAsync),

                    ButtonKit(State.Counter == 0
                                ? () => TranslationResources.ButtonStaticText
                                : () => string.Format(TranslationResources.ButtonClickedTimesText, State.Counter))
                        .ThemeKey(ApplicationTheme.Primary)
                        .OnClicked(NavigateToSecondPageAsync),

                    ButtonKit(State.Counter == 0
                                ? () => TranslationResources.ButtonStaticText
                                : () => string.Format(TranslationResources.ButtonClickedTimesText, State.Counter))
                        .ThemeKey(ApplicationTheme.Primary)
                        .OnClicked(NavigateToSecondPageAsync),

                    ButtonKit(State.Counter == 0
                                ? () => TranslationResources.ButtonStaticText
                                : () => string.Format(TranslationResources.ButtonClickedTimesText, State.Counter))
                        .ThemeKey(ApplicationTheme.Primary)
                        .OnClicked(NavigateToSecondPageAsync)
                )
                .VCenter()
                .Spacing(25)
                .Padding(30, 0)
            )
        );

    public ValueTask OnAppearingAsync()
    {
        if (!string.IsNullOrEmpty(Props?.PopInfo))
        {
            Console.WriteLine($"This info has been received from the inner page after popping: {Props.PopInfo}");
        }
        
        return ValueTask.CompletedTask;
    }

    private Task NavigateToSecondPageAsync() =>
        _navigation.GoToAsync(
            Nalu.Navigation.Relative().Push<SecondPage>().WithPropsDelegate<SecondPageProps>(o => o.Id = 42));

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposed = true;
        }
    }
}