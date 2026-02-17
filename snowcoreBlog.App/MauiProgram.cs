using FFImageLoading.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
#if ANDROID
using snowcoreBlog.App.Platforms.Android.Handlers;
#endif
using Plugin.Maui.BottomSheet.Hosting;
using ReactorTheme;
using snowcoreBlog.App.Extensions;
using snowcoreBlog.App.Features.BlogAppShell;
using snowcoreBlog.App.Features.Home;
using snowcoreBlog.App.Features.Second;
using snowcoreBlog.App.Features.Settings;
using snowcoreBlog.App.Features.TabThree;
using snowcoreBlog.App.Features.TabTwo;
using snowcoreBlog.App.Features.Third;
using snowcoreBlog.App.Resources.Styles;
using snowcoreBlog.App.Services.Background;

namespace snowcoreBlog.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiReactorApp<AppShell>(static app =>
            {
                app.UseTheme<snowcoreBlogApplicationTheme>();
            },
            unhandledExceptionAction: static e =>
            {
                System.Diagnostics.Debug.WriteLine(e.ExceptionObject);
            })
            .UseReactorThemeFonts()
            .UseMauiCommunityToolkit()
            .UseInsets()
            .UseBottomSheet()
            .UseFFImageLoading()
            .ConfigureMauiHandlers(handlers =>
            {
#if ANDROID
                handlers.AddHandler<Microsoft.Maui.Controls.Shell, CustomShellRenderer2>();
#endif
            })
            .ConfigureFonts(static fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder
            .UseNaluNavigation(configure: configurator =>
            {
                configurator
                    .SetRoot<HomePage>()
                    .AddPage<SecondPage>()
                    .AddPage<TabTwoPage>()
                    .AddPage<ThirdPage>()
                    .AddPage<TabThreePage>()
                    .AddPage<SettingsPage>()
                    .WithLeakDetectorState(Nalu.NavigationLeakDetectorState.EnabledWithDebugger);
            });

        builder.Configuration.AddConfiguration(GetAppSettingsConfig(TranslationResources.snowcoreBlogAppSettingsJson));
        builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
        builder.Logging.AddConsole();

#if DEBUG
        builder.Configuration.AddConfiguration(GetAppSettingsConfig(TranslationResources.snowcoreBlogAppSettingsDebugJson));
        builder.Logging.AddDebug();
#endif

        builder.Logging.AddEventSourceLogger();

        builder.Services.AddOptions();

        // Register platform-specific background service
#if ANDROID
        builder.Services.AddSingleton<IPlatformBackgroundService, Platforms.Android.Services.AndroidPlatformBackgroundService>();
#elif IOS
        builder.Services.AddSingleton<IPlatformBackgroundService, Platforms.iOS.Services.iOSPlatformBackgroundService>();
#elif MACCATALYST
        builder.Services.AddSingleton<IPlatformBackgroundService, Platforms.MacCatalyst.Services.MacCatalystPlatformBackgroundService>();
#elif WINDOWS
        builder.Services.AddSingleton<IPlatformBackgroundService, Platforms.Windows.Services.WindowsPlatformBackgroundService>();
#endif

        // Register the main background service
        builder.Services.AddHostedService<SampleBackgroundService>();
        
        return builder.Build();
    }

    private static IConfigurationRoot GetAppSettingsConfig(string resourceUri)
    {
        using var appSettingsStream = typeof(MauiProgram).Assembly.GetManifestResourceStream(resourceUri);
        if (appSettingsStream == null)
        {
            return new ConfigurationBuilder().Build();
        }

        return new ConfigurationBuilder().AddJsonStream(appSettingsStream).Build();
    }
}