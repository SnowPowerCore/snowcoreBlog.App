using Foundation;
using Nalu;
using UIKit;

namespace snowcoreBlog.App
{
    [Register(nameof(AppDelegate))]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        [Export("application:handleEventsForBackgroundURLSession:completionHandler:")]
        public virtual void HandleEventsForBackgroundUrl(UIApplication application, string sessionIdentifier, Action completionHandler) =>
            NSUrlBackgroundSessionHttpMessageHandler.HandleEventsForBackgroundUrl(application, sessionIdentifier, completionHandler);
    }
}