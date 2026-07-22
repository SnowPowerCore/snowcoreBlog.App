using System.Globalization;
using MauiReactor;
using MauiReactor.Parameters;
using Microsoft.Extensions.Configuration;
using Mocale.Abstractions;
using Mocale.Translations;
using Nalu.Interfaces;
using ReactorTheme.Styles;
using snowcoreBlog.App.Components;
using snowcoreBlog.App.Features.BlogAppShell;
using snowcoreBlog.App.Features.Second;
using snowcoreBlog.App.Resources;

namespace snowcoreBlog.App.Features.Home;

public partial class HomePage : Component<HomePageState>, IAppearingAware<HomePageProps>, IDisposable
{
    private bool _disposed = false;
    
    private IParameter<AppShellSettings> _appShellSettings;

    [Inject]
    private readonly IConfiguration _configuration;

    [Inject]
    private readonly INavigationService _navigation;

    [Inject]
    private readonly ITranslatorManager _translatorManager;

    [Inject]
    private readonly ILocalizationManager _localizationManager;

    public override VisualNode Render()
    {
        _appShellSettings = GetParameter<AppShellSettings>();

        return CustomContentPage(title: _translatorManager.Translate(TranslationKeys.HomeShellTitle), children:
            DelayedView(
                ScrollView(
                    VStack(
                        CachedImage(TranslationResources.DotNetBotImgSrc)
                            .DownsampleToViewSize(true)
                            .HeightRequest(200)
                            .HCenter()
                            .Set(SemanticProperties.DescriptionProperty, _translatorManager.Translate(TranslationKeys.SemanticDotNetBotDesc)),

                        Label(_configuration[TranslationResources.AppNameKey])
                            .ThemeKey(ApplicationTheme.H1)
                            .HCenter(),

                        Label(_translatorManager.Translate(TranslationKeys.SubheaderText))
                            .ThemeKey(ApplicationTheme.H3)
                            .HCenter(),

                        Label(_translatorManager.Translate(TranslationKeys.HomeLoremTitle))
                            .FontSize(24)
                            .LineHeight(1.5),

                        Label(_translatorManager.Translate(TranslationKeys.HomeLoremExplain)),

                        Label(_translatorManager.Translate(TranslationKeys.HomeLoremSample)),

                        ButtonKit(GetClickedTimes())
                            .ThemeKey(ApplicationTheme.Primary)
                            .OnClicked(NavigateToSecondPageAsync),

                        ButtonKit(GetClickedTimes())
                            .ThemeKey(ApplicationTheme.Secondary)
                            .OnClicked(NavigateToSecondPageAsync),

                        ButtonKit(GetClickedTimes())
                            .ThemeKey(ApplicationTheme.Primary)
                            .OnClicked(NavigateToSecondPageAsync),

                        ButtonKit(GetClickedTimes())
                            .ThemeKey(ApplicationTheme.Primary)
                            .OnClicked(NavigateToSecondPageAsync),

                        ButtonKit(GetClickedTimes())
                            .ThemeKey(ApplicationTheme.Primary)
                            .OnClicked(NavigateToSecondPageAsync),

                        ButtonKit(GetClickedTimes())
                            .ThemeKey(ApplicationTheme.Primary)
                            .OnClicked(NavigateToSecondPageAsync),

                        ButtonKit(GetClickedTimes())
                            .ThemeKey(ApplicationTheme.Primary)
                            .OnClicked(NavigateToSecondPageAsync),

                        ButtonKit(GetClickedTimes())
                            .ThemeKey(ApplicationTheme.Primary)
                            .OnClicked(NavigateToSecondPageAsync),

                        ButtonKit(() => TranslationResources.SwitchToRussianText)
                            .ThemeKey(ApplicationTheme.Primary)
                            .OnClicked(ChangeCultureToRussianAsync),

                        ButtonKit(() => TranslationResources.SwitchToTurkishText)
                            .ThemeKey(ApplicationTheme.Primary)
                            .OnClicked(ChangeCultureToTurkishAsync),

                        ButtonKit(() => TranslationResources.ResetLanguageText)
                            .ThemeKey(ApplicationTheme.Primary)
                            .OnClicked(ResetLanguageAsync)
                    )
                    .VCenter()
                    .Spacing(25)
                    .Padding(30, 0, 30, 60)
                    .PadBottom()
                )
            )
            .UseActivityIndicator(true)
        );
    }

    private Func<string> GetClickedTimes() =>
        State.Counter == 0
            ? () => _translatorManager.Translate(TranslationKeys.ButtonStaticText)
            : () => string.Format(_translatorManager.Translate(TranslationKeys.ButtonClickedTimesText), State.Counter);

    public ValueTask OnAppearingAsync(HomePageProps intent)
    {
        if (!string.IsNullOrEmpty(intent?.PopInfo))
        {
            Console.WriteLine($"This info has been received from the inner page after popping: {intent.PopInfo}");
        }

        return ValueTask.CompletedTask;
    }

    private Task ChangeCultureToRussianAsync() => ChangeCultureToAsync("ru-RU");

    private Task ChangeCultureToTurkishAsync() => ChangeCultureToAsync("tr-TR");

    private Task ResetLanguageAsync() => ChangeCultureToAsync("en");

    private async Task ChangeCultureToAsync(string localeStr)
    {
        var culture = new CultureInfo(localeStr);

        var changed = await _localizationManager.SetCultureAsync(culture);
        _appShellSettings?.Value.NotifyCurrentCultureChanged();

        Console.WriteLine($"Changed to {localeStr}: {changed}");

        Invalidate();
    }

    private Task NavigateToSecondPageAsync() =>
        _navigation.GoToAsync(
            Nalu.NavigationInfo.Navigation.Relative().Push<SecondPage>().WithIntent(new SecondPageProps { Id = 42 }));

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
