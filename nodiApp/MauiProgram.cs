using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using nodiApp.Services;
using nodiApp.ViewModels;
using nodiApp.Views;

namespace nodiApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Services
            builder.Services.AddSingleton<LocalDbService>();
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddSingleton<SyncService>();

            // ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<NotesViewModel>();
            builder.Services.AddTransient<NoteDetailViewModel>();

            // Pages
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<NotesPage>();
            builder.Services.AddTransient<NoteDetailPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
