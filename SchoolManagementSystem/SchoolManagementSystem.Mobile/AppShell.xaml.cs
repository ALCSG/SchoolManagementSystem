using Microsoft.Extensions.DependencyInjection;
using SchoolManagementSystem.Mobile.Views;

namespace SchoolManagementSystem.Mobile
{
    public partial class AppShell : Shell
    {
        private bool _isLoggingOut;

        public AppShell(IServiceProvider services)
        {
            InitializeComponent();

            LearningContent.ContentTemplate = new DataTemplate(
                () => services.GetRequiredService<MyLearningPage>());

            ProfileContent.ContentTemplate = new DataTemplate(
                () => services.GetRequiredService<ProfilePage>());

            AboutContent.ContentTemplate = new DataTemplate(
                () => services.GetRequiredService<AboutPage>());
        }

        private async void OnLogoutClicked(object? sender,EventArgs e)
        {
            if (_isLoggingOut)
                return;

            _isLoggingOut = true;

            try
            {
                FlyoutIsPresented = false;

                var confirmed = await DisplayAlertAsync(
                    "Log out",
                    "Do you want to sign out?",
                    "Yes",
                    "No");

                if (!confirmed)
                    return;

                if (Application.Current is App app &&
                    Window is Window window)
                {
                    await app.SignOutAsync(window);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                await DisplayAlertAsync(
                    "Log out",
                    "Could not sign out. Please try again.",
                    "OK");
            }
            finally
            {
                _isLoggingOut = false;
            }
        }
    }
}