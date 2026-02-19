using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using ShellSection = Microsoft.Maui.Controls.ShellSection;

namespace snowcoreBlog.App.Platforms.iOS.Handlers;

public class CustomShellRenderer : ShellRenderer
{
    protected override IShellItemTransition CreateShellItemTransition() =>
        new CustomShellItemTransition();

    protected override IShellSectionRenderer CreateShellSectionRenderer(ShellSection shellSection) =>
        new CustomShellSectionRenderer(this);
}