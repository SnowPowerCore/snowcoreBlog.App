using Microsoft.Maui.LifecycleEvents;

namespace snowcoreBlog.App.Extensions;

public static class InsetsAppBuilderExtensions
{
    public static MauiAppBuilder UseInsets(this MauiAppBuilder builder)
    {
        builder.ConfigureLifecycleEvents(lifecycle =>
        {
#if ANDROID
            lifecycle.AddAndroid(androidLifecycle =>
            {
                androidLifecycle.OnApplicationCreate((application) =>
                {
                    if (application is IPlatformApplication platformApp)
                    {
                        var appInterface = platformApp.Services.GetService<IApplication>();

                        if (appInterface is Application app)
                        {
                            Insets.Current.Init(app.MainPage);
                        }
                    }
                })
                .OnPostCreate((activity, bundle) =>
                {
                    Insets.Current.InitActivity(activity);
                });
            });
#elif IOS || MACCATALYST
            lifecycle.AddiOS(iOSLifecycle =>
            {
                iOSLifecycle.FinishedLaunching((application, bundle) =>
                {
                    if (application.Delegate is IPlatformApplication platformApp)
                    {
                        var appInterface = platformApp.Services.GetService<IApplication>();

                        if (appInterface is Application app)
                        {
                            Insets.Current.Init(app.MainPage);
                        }
                    }
                    return true;
                });
            });
#elif WINDOWS
        lifecycle.AddWindows(windowsLifecycle =>
            {
                windowsLifecycle.OnLaunched((application, bundle) =>
                {
                    if (application is IPlatformApplication platformApp)
                    {
                        var appInterface = platformApp.Services.GetService<IApplication>();

                        if (appInterface is Application app)
                        {
                            Insets.Current.Init(app.MainPage);
                        }
                    }
                });
            });
#endif
        });
        return builder;
    }
}