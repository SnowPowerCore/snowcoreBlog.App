using Nalu.Reactor;
using snowcoreBlog.App.Components;
using snowcoreBlog.App.Components.Overriden;

namespace snowcoreBlog.App.Extensions;

public static class ViewExtensions
{
    public static BottomSheet BottomSheet(params IEnumerable<VisualNode?>? children) => new(children);

    public static CachedImage CachedImage(string source = "") => new CachedImage().Source(source);

    public static ButtonKit ButtonKit(string text = "") => new ButtonKit().Text(text);

    public static ButtonKit ButtonKit(Func<string>? textFunc = null) => new ButtonKit().TextFunc(textFunc);

    public static NaluReactorShell AppShellControl(params IEnumerable<VisualNode?>? children) => new(children);
}