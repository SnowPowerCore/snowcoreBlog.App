namespace snowcoreBlog.App.Features.BlogAppShell;

public class AppShellSettings
{
    public event EventHandler? CurrentCultureChanged;

    public void NotifyCurrentCultureChanged()
    {
        CurrentCultureChanged?.Invoke(this, EventArgs.Empty);
    }
}