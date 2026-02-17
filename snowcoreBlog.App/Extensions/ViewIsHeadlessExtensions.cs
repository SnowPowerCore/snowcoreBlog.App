namespace snowcoreBlog.App.Extensions;

public static class ViewIsHeadlessExtensions
{
    public static MauiReactor.Grid IsHeadless(this MauiReactor.Grid visualNode, bool value = true) =>
        visualNode.Set(CompressedLayout.IsHeadlessProperty, value);

    public static MauiReactor.ContentView IsHeadless(this MauiReactor.ContentView visualNode, bool value = true) =>
        visualNode.Set(CompressedLayout.IsHeadlessProperty, value);
}