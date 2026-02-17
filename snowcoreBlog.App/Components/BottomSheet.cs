using MauiReactor.Internals;
using Plugin.Maui.BottomSheet.Navigation;

namespace snowcoreBlog.App.Components;

[Scaffold(typeof(Plugin.Maui.BottomSheet.BottomSheet))]
public partial class BottomSheet
{
    protected override void OnAddChild(VisualNode widget, BindableObject childControl)
    {
        if (childControl is View childView)
        {
            Validate.EnsureNotNull(NativeControl);
            NativeControl.Content = new Plugin.Maui.BottomSheet.BottomSheetContent { ContentTemplate = new DataTemplate(() => childView) };
        }
        else
        {
            base.OnAddChild(widget, childControl);
        }
    }
};

public static class BottomSheetNavigationServiceMauiReactorExtensions
{
    public static async Task OpenBottomSheetAsync(this IBottomSheetNavigationService bottomSheetNavigationService, Func<BottomSheet> contentRender)
    {
        var templateHost = TemplateHost.Create(contentRender());
        var bottomSheet = (Plugin.Maui.BottomSheet.IBottomSheet)templateHost.NativeElement.EnsureNotNull();

        bottomSheet.ClosedCommand = new Command(OnBottomSheetClosed)!;
        void OnBottomSheetClosed()
        {
            (templateHost as IHostElement)?.Stop();
        }

        await bottomSheetNavigationService.NavigateToAsync(bottomSheet);
    }
}