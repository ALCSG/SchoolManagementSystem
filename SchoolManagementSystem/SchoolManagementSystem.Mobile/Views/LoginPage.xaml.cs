using System.Diagnostics;
using System.Text.Json;
using SchoolManagementSystem.Mobile.Services;

namespace SchoolManagementSystem.Mobile.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly AuthService _authService;
        private readonly StudentService _studentService;
        private bool _isBusy;

        public LoginPage(AuthService authService, StudentService studentService)
        {
            InitializeComponent();
            _authService = authService;
            _studentService = studentService;
        }

        private void OnEmailCompleted(object? sender, EventArgs e)
        {
            PasswordEntry.Focus();
        }

        private async void OnLoginClicked(object? sender, EventArgs e)
        {
            if (_isBusy)
                return;

            ErrorLabel.IsVisible = false;

            var email = EmailEntry.Text?.Trim() ?? string.Empty;
            var password = PasswordEntry.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ShowError("Please enter your email and password.");
                return;
            }

            SetBusy(true);

            try
            {
                var result = await _authService.LoginAsync(
                    email,
                    password);

                PasswordEntry.Text = string.Empty;

                await Navigation.PushAsync(new MyLearningPage(_studentService, _authService));
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine(
                    $"Login HTTP failure: {ex.GetType().Name}");

                ShowError(
                    "Could not complete the request. " +
                    "Check your connection and try again.");
            }
            catch (OperationCanceledException)
            {
                ShowError(
                    "The request timed out. Please try again.");
            }
            catch (JsonException)
            {
                ShowError(
                    "The server returned an unexpected response.");
            }
            catch (InvalidOperationException ex)
            {
                ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Login failure: {ex.GetType().Name}");

                ShowError(
                    "Login could not be completed. " +
                    "Check that secure storage is available and try again.");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void ShowError(string message)
        {
            ErrorLabel.Text = message;
            ErrorLabel.IsVisible = true;
        }

        private void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;

            LoginButton.IsEnabled = !isBusy;
            EmailEntry.IsEnabled = !isBusy;
            PasswordEntry.IsEnabled = !isBusy;

            LoadingIndicator.IsVisible = isBusy;
            LoadingIndicator.IsRunning = isBusy;
        }
    }
}