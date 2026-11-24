using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Mobile.Services;
using SchoolManagementSystem.Mobile.Views;
using System.Net.Http.Headers;

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
            builder.Services.AddSingleton<HttpClient>(_ =>
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri("https://schoolmanagemtnsys107.runasp.net/"),
                    Timeout = TimeSpan.FromSeconds(30)
                };

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Bearer",
                        SecureStorage.Default.GetAsync(AuthService.AccessTokenKey).GetAwaiter().GetResult() ?? string.Empty);

                return client;
            });

            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<StudentService>();
            builder.Services.AddSingleton<ProfileService>();

            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<MyLearningPage>();
            builder.Services.AddTransient<ProfilePage>();

            return builder.Build();
        }
    }
}
