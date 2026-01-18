using Microsoft.Extensions.Logging;
using MauiIcons.Fluent;
using KnolageTests.Services;

namespace KnolageTests
{
    public static class MauiProgram
    {
        internal static IServiceProvider Services;

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                   
                });
            builder.UseMauiApp<App>().UseFluentMauiIcons();


            builder.Services.AddSingleton<TestAttemptDatabaseService>(s =>
            {
                string dbPath = Path.Combine(FileSystem.AppDataDirectory, "knowly.db");
                return new TestAttemptDatabaseService(dbPath);
            });

            builder.Services.AddSingleton<TestsService>();
            builder.Services.AddSingleton<ImageStorageService>();

#if ANDROID
            builder.Services.AddSingleton<INotificationService, AndroidNotificationService>();
#else
            builder.Services.AddSingleton<INotificationService, DummyNotificationService>();
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();
            Services = app.Services;
            return app;
        }
    }
}
