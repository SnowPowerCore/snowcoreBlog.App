using Nalu;
using snowcoreBlog.App.Features.Home;
using snowcoreBlog.App.Features.Third;

namespace snowcoreBlog.App.Features.Second;

public partial class SecondPage(INavigationService navigation, INavigationServiceProvider navigationServiceProvider) : ComponentWithProps<SecondPageProps>, IAppearingAware, ILeavingGuard, IDisposable
{
    private bool _disposed = false;

    private readonly INavigationService _navigation = navigation;
    private readonly INavigationServiceProvider _navigationServiceProvider = navigationServiceProvider;

    public override VisualNode Render() =>
        CustomContentPage(TranslationResources.SecondPageTitle, children:
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
        );

    public ValueTask OnAppearingAsync()
    {
        Console.WriteLine($"Navigated to {nameof(SecondPage)} with Id: {Props.Id}");
        _ = _navigationServiceProvider.ContextPage;
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> CanLeaveAsync() =>
        ValueTask.FromResult(true);

    private Task NavigateToFirstPageAsync() =>
        _navigation.GoToAsync(Nalu.Navigation.Relative().Pop().WithPropsDelegate<HomePageProps>(o => o.PopInfo = "Hello world!"));

    private Task NavigateToThirdPageAsync() =>
        _navigation.GoToAsync(Nalu.Navigation.Relative().Push<ThirdPage>());

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