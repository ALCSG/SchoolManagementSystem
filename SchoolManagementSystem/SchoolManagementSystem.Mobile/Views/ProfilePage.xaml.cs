using Microsoft.Maui.Storage;
using SchoolManagementSystem.Mobile.Services;

namespace SchoolManagementSystem.Mobile.Views
{
    public partial class ProfilePage : ContentPage
    {
        private const int MaxPhotoSizeBytes = 2 * 1024 * 1024;

        private readonly ProfileService _profileService;

        private bool _isBusy;
        private bool _profileLoaded;

        private byte[]? _selectedPhotoBytes;
        private string? _selectedPhotoFileName;

        public ProfilePage(ProfileService profileService)
        {
            InitializeComponent();
            _profileService = profileService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!_profileLoaded && !_isBusy)
            {
                await LoadProfileAsync();
            }
        }

        private async Task LoadProfileAsync()
        {
            ErrorLabel.IsVisible = false;
            ProfileLayout.IsVisible = false;

            SetBusy(true);

            try
            {
                var profile = await _profileService.GetProfileAsync();

                FullNameEntry.Text = profile.FullName;
                EmailEntry.Text = profile.Email;
                PhoneNumberEntry.Text = profile.PhoneNumber;

                var photoUrl = _profileService.GetPhotoUrl(
                    profile.PhotoUrl);

                ProfilePhoto.Source = photoUrl == null
                    ? null
                    : ImageSource.FromUri(new Uri(photoUrl));

                ProfilePhoto.IsVisible = photoUrl != null;
                NoPhotoLabel.IsVisible = photoUrl == null;

                _profileLoaded = true;
                ProfileLayout.IsVisible = true;
            }
            catch (UnauthorizedAccessException)
            {
                await HandleExpiredSessionAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                ShowError("Could not load your profile.");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void OnChoosePhotoClicked(
            object? sender,
            EventArgs e)
        {
            if (_isBusy)
                return;

            ErrorLabel.IsVisible = false;
            SetBusy(true);

            try
            {
                var file = await FilePicker.Default.PickAsync(
                    new PickOptions
                    {
                        PickerTitle = "Choose a profile photo",
                        FileTypes = FilePickerFileType.Images
                    });

                if (file == null)
                    return;

                var extension = Path.GetExtension(file.FileName)
                    .ToLowerInvariant();

                if (extension != ".jpg"
                    && extension != ".jpeg"
                    && extension != ".png")
                {
                    ShowError("Only JPG, JPEG and PNG images are allowed.");
                    return;
                }

                using var input = await file.OpenReadAsync();
                using var output = new MemoryStream();

                var buffer = new byte[8192];
                int bytesRead;

                while ((bytesRead = await input.ReadAsync(
                    buffer.AsMemory(0, buffer.Length))) > 0)
                {
                    if (output.Length + bytesRead > MaxPhotoSizeBytes)
                    {
                        ShowError("The image must not exceed 2 MB.");
                        return;
                    }

                    await output.WriteAsync(
                        buffer.AsMemory(0, bytesRead));
                }

                if (output.Length == 0)
                {
                    ShowError("Please select a non-empty image.");
                    return;
                }

                var photoBytes = output.ToArray();

                _selectedPhotoBytes = photoBytes;
                _selectedPhotoFileName = file.FileName;

                // Each image load receives a new readable stream.
                ProfilePhoto.Source = ImageSource.FromStream(
                    () => new MemoryStream(photoBytes));

                ProfilePhoto.IsVisible = true;
                NoPhotoLabel.IsVisible = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                ShowError(
                    "Could not select the photo. Please try again.");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void OnSaveClicked(
            object? sender,
            EventArgs e)
        {
            if (_isBusy)
                return;

            ErrorLabel.IsVisible = false;
            SetBusy(true);

            try
            {
                await _profileService.UpdateProfileAsync(
                    PhoneNumberEntry.Text?.Trim(),
                    _selectedPhotoBytes,
                    _selectedPhotoFileName);

                _selectedPhotoBytes = null;
                _selectedPhotoFileName = null;

                await DisplayAlertAsync(
                    "Profile updated",
                    "Your profile was updated successfully.",
                    "OK");
            }
            catch (UnauthorizedAccessException)
            {
                await HandleExpiredSessionAsync();
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                var message = ex.StatusCode.HasValue
                    ? $"Could not update your profile "
                        + $"(HTTP {(int)ex.StatusCode.Value})."
                    : "Could not connect to the server. Please try again.";

                ShowError(message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                ShowError(
                    "Could not update your profile. Please try again.");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task HandleExpiredSessionAsync()
        {
            await DisplayAlertAsync(
                "Session expired",
                "Please sign in again.",
                "OK");

            await Navigation.PopToRootAsync();
        }

        private void ShowError(string message)
        {
            ErrorLabel.Text = message;
            ErrorLabel.IsVisible = true;
        }

        private void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;

            SaveButton.IsEnabled = !isBusy;
            ChoosePhotoButton.IsEnabled = !isBusy;
            PhoneNumberEntry.IsEnabled = !isBusy;

            LoadingIndicator.IsVisible = isBusy;
            LoadingIndicator.IsRunning = isBusy;
        }
    }
}