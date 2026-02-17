using MauiReactor.Compatibility;
using Nalu.Reactor;
using snowcoreBlog.App.Components;
using snowcoreBlog.App.Components.Overriden;

namespace snowcoreBlog.App.Extensions;

public static class ViewExtensions
{
    public static VisualNode CustomContentPage(
        string? title = null,
        VisualNode? titleBarView = null,
        Func<bool>? isBackButtonVisible = null,
        Func<Task>? onNavigatedBack = null,
        params IEnumerable<VisualNode?>? children) =>
        new CustomContentPage()
            .Title(title)
            .TitleBarView(() => titleBarView ?? new MauiReactor.ContentView().IsHeadless())
            .İsPreviousPageButtonVisible(isBackButtonVisible?.Invoke())
            .Content(() => children)
            .OnNavigatedBack(onNavigatedBack);

    public static BottomSheet BottomSheet(params IEnumerable<VisualNode?>? children) => new(children);

    public static TopInsetView TopInsetsView(params IEnumerable<VisualNode?>? children) => new(children);
    
    public static BottomInsetView BottomInsetsView(params IEnumerable<VisualNode?>? children) => new(children);

    public static VStack PadBottom(this VStack vStack)
    {
        vStack.AddChildren(new BottomInsetView());
        return vStack;
    }

    public static CachedImage CachedImage(string source = "") => new CachedImage().Source(source);

    public static SvgCachedImage SvgCachedImage(string source = "") => new SvgCachedImage().Source(source);

    public static ButtonKit ButtonKit(string text = "") => new ButtonKit().Text(text);

    public static ButtonKit ButtonKit(Func<string>? textFunc = null) => new ButtonKit().TextFunc(textFunc);

    public static NaluReactorShell AppShellControl(params IEnumerable<VisualNode?>? children) => new(children);

    public static MauiReactor.ContentPage ShellNavBarIsVisible(this MauiReactor.ContentPage contentPage, bool value) =>
        contentPage.Set(MauiControls.Shell.NavBarIsVisibleProperty, value);
    
    public static MauiReactor.ContentPage EdgeToEdge(this MauiReactor.ContentPage contentPage, bool value = true) =>
        contentPage.Set(Insets.EdgeToEdgeProperty, value);
}