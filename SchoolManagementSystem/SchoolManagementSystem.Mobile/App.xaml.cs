using Microsoft.Extensions.DependencyInjection;
using SchoolManagementSystem.Mobile.Services;
using SchoolManagementSystem.Mobile.Views;

namespace SchoolManagementSystem.Mobile
{
    public partial class App : Application
    {
        private readonly IServiceProvider _services;
        private bool _isSigningOut;

        public App(IServiceProvider services)
        {
            InitializeComponent();
            _services = services;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(CreateLoginPage());
        }

        private NavigationPage CreateLoginPage()
        {
            return new NavigationPage(_services.GetRequiredService<LoginPage>())
            {
                BarBackgroundColor = Color.FromArgb("#09233f"),
                BarTextColor = Colors.White
            };
        }

        public void ShowMainArea(Window window)
        {
            window.Page = _services.GetRequiredService<AppShell>();
        }

        public async Task SignOutAsync(Window window)
        {
            if (_isSigningOut)
                return;

            _isSigningOut = true;

            try
            {
                var authService = _services.GetRequiredService<AuthService>();

                await authService.LogoutAsync();

                window.Page = CreateLoginPage();
            }
            finally
            {
                _isSigningOut = false;
            }
        }
    }
}