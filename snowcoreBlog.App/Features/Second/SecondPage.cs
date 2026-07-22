using MauiReactor;
using Nalu.Interfaces;
using ReactorTheme.Styles;
using snowcoreBlog.App.Components;
using snowcoreBlog.App.Features.Home;
using snowcoreBlog.App.Features.Third;
using snowcoreBlog.App.Resources;

namespace snowcoreBlog.App.Features.Second;

public partial class SecondPage(INavigationService navigation, INavigationServiceProvider navigationServiceProvider) : Component, IAppearingAware<SecondPageProps>, ILeavingGuard, IDisposable
{
    private bool _disposed = false;

    private readonly INavigationService _navigation = navigation;
    private readonly INavigationServiceProvider _navigationServiceProvider = navigationServiceProvider;

    public override VisualNode Render() =>
        CustomContentPage(TranslationResources.SecondPageTitle, children:
            DelayedView(
                VStack(
                    ButtonKit(() => TranslationResources.GoToFirstPageText)
                        .HorizontalOptions(LayoutOptions.Center)
                        .ThemeKey(ApplicationTheme.Primary)
                        .OnClicked(NavigateToFirstPageAsync),

                    ButtonKit(() => TranslationResources.GoToThirdPageText)
                        .ThemeKey(ApplicationTheme.Primary)
                        .OnClicked(NavigateToThirdPageAsync)
                )
                .VCenter()
            )
            .UseActivityIndicator(true)
        );

    public ValueTask OnAppearingAsync(SecondPageProps intent)
    {
        Console.WriteLine($"Navigated to {nameof(SecondPage)} with Id: {intent.Id}");
        _ = _navigationServiceProvider.ContextPage;
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> CanLeaveAsync() =>
        ValueTask.FromResult(true);

    private Task NavigateToFirstPageAsync() =>
        _navigation.GoToAsync(Nalu.NavigationInfo.Navigation.Relative().Pop().WithIntent(new HomePageProps { PopInfo = TranslationResources.HelloWorld }));

    private Task NavigateToThirdPageAsync() =>
        _navigation.GoToAsync(Nalu.NavigationInfo.Navigation.Relative().Push<ThirdPage>());

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