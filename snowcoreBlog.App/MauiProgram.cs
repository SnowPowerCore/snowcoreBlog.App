using CommunityToolkit.Maui;
using FFImageLoading.Maui;
using HotReloadSentinel.Diagnostics;
using MauiReactor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mocale;
using Mocale.Cache.SQLite;
using Nalu.Extensions;
using Nalu.Interfaces;
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
using snowcoreBlog.App.Resources;
using snowcoreBlog.App.Resources.Styles;
using snowcoreBlog.App.Services.Background;

#if ANDROID
using snowcoreBlog.App.Platforms.Android.Handlers;
#endif

#if IOS
using snowcoreBlog.App.Platforms.iOS.Handlers;
#endif

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
            .UseMocale(mocale =>
            {
                mocale
                    .WithConfiguration(static config =>
                    {
                        config.UseExternalProvider = false;
                    })
                    .UseEmbeddedResources(static config =>
                    {
                        config.ResourcesPath = "Locales";
                        config.ResourcesAssembly = typeof(MauiProgram).Assembly;
                    })
                    .UseSqliteCache(static config =>
                    {
                        config.UpdateInterval = TimeSpan.FromMinutes(15);
                    });
            })
            .ConfigureMauiHandlers(handlers =>
            {
#if ANDROID
                handlers.AddHandler<MauiControls.Shell, CustomShellRenderer2>();

#elif IOS
                handlers.AddHandler<MauiControls.Shell, CustomShellRenderer>();
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
                    .AddComponent<HomePage>()
                    .AddComponent<SecondPage>()
                    .AddComponent<TabTwoPage>()
                    .AddComponent<ThirdPage>()
                    .AddComponent<TabThreePage>()
                    .AddComponent<SettingsPage>()
                    .WithLeakDetectorState(NavigationLeakDetectorState.EnabledWithDebugger);
            });

        builder.Configuration.AddConfiguration(GetAppSettingsConfig(TranslationResources.snowcoreBlogAppSettingsJson));
        builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
        builder.Logging.AddConsole();
        
#if DEBUG
        builder.Configuration.AddConfiguration(GetAppSettingsConfig(TranslationResources.snowcoreBlogAppSettingsDebugJson));
        builder.Logging.AddDebug();
        builder.UseHotReloadDiagnostics();
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
        return appSettingsStream is not default(Stream)
            ? new ConfigurationBuilder().AddJsonStream(appSettingsStream).Build()
            : new ConfigurationBuilder().Build();
    }
}