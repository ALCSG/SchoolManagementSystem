using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Mobile.Services;
using SchoolManagementSystem.Mobile.Views;

namespace SchoolManagementSystem.Mobile
{
    public static class MauiProgram
    {
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

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<HttpClient>(_ => new HttpClient
                {
                    BaseAddress = new Uri("https://schoolmanagemtnsys107.runasp.net/"),Timeout = TimeSpan.FromSeconds(30)
                });

            builder.Services.AddSingleton<AuthService>();

            builder.Services.AddTransient<LoginPage>();

            return builder.Build();
        }
    }
}
